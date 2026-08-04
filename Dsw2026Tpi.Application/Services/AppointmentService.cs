using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IPersistence _persistence;
        private readonly ILogger<AppointmentService> _logger;
        public AppointmentService(IPersistence persistence, ILogger<AppointmentService> logger)
        {
            _persistence = persistence;
            _logger = logger;
        }
        public async Task<AppointmentModel.Response> Book(AppointmentModel.Request request)
        {
            Validate(request);
            var doctor = await _persistence.GetById<Doctor>(request.DoctorId)
                ?? throw new EntityNotFoundException(nameof(Doctor));
            var slot = await _persistence.GetById<Slot>(request.AvailabilitySlotId, nameof(Slot.Availability))
                 ?? throw new EntityNotFoundException(nameof(Slot));
            if (slot.Availability!.DoctorId != request.DoctorId)
                throw new ValidationException("El turno no pertenece al médico indicado", "APPOINTMENT_DOCTOR_MISMATCH");
            var today = DateOnly.FromDateTime(DateTime.Today);
            if (slot.Date < today || (slot.Date == today && slot.StartTime < TimeOnly.FromDateTime(DateTime.Now)))
                throw new BusinessRuleException("No se pueden reservar turnos en el pasado", "APPOINTMENT_PAST_DATE");
            var patient = await _persistence.First<Patient>(p => p.Dni == request.Patient.Dni)
                ?? throw new ValidationException("El paciente indicado no existe", "APPOINTMENT_PATIENT_NOT_FOUND");
            if (slot.Status != SlotStatus.Available)
                throw new ConflictException("APPOINTMENT_CONFLICT", "El turno ya fue reservado");
            slot.Book();
            try
            {
                await _persistence.Update(slot);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException("APPOINTMENT_CONFLICT", "El turno ya fue reservado");
            }
            var appointment = new Appointment(slot.Id, patient.Id, request.Reason);
            await _persistence.Add(appointment);
            _logger.LogInformation("Turno reservado: AppointmentId={AppointmentId}, SlotId={SlotId}, PatientDni={Dni}",
            appointment.Id, slot.Id, request.Patient.Dni);
            return new AppointmentModel.Response(appointment.Id, appointment.Status.ToString());
        }
        public async Task Cancel(Guid id)
        {
            var appointment = await _persistence.GetByIdBase<Appointment>(id)
                ?? throw new EntityNotFoundException(nameof(Appointment));
            if (appointment.Status != AppointmentStatus.Booked)
                throw new BusinessRuleException("Solo se puede cancelar un turno en estado BOOKED", "APPOINTMENT_NOT_CANCELLABLE");
            appointment.Cancel();
            await _persistence.Update(appointment);
            var slot = await _persistence.GetById<Slot>(appointment.SlotId)
                ?? throw new EntityNotFoundException(nameof(Slot));
            slot.Free();
            _logger.LogInformation("Turno cancelado: AppointmentId={AppointmentId}", id);
            await _persistence.Update(slot);
        }
        public async Task<IEnumerable<AppointmentModel.PatientAppointmentDto>> GetByPatient(long dni)
        {
            var today = DateTime.Today;
            var patient = await _persistence.First<Patient>(p => p.Dni == dni)
                ?? throw new EntityNotFoundException(nameof(Patient));
            var appointments = await _persistence.GetFiltered<Appointment>(a =>
                a.PatientId == patient.Id && a.Status == AppointmentStatus.Booked && a.CreatedAt > today,
                "AvailabilitySlot.Availability.Doctor.Speciality") ?? [];
            return appointments.Select(a => new AppointmentModel.PatientAppointmentDto(
                a.Id,
                a.AvailabilitySlot!.Availability!.Doctor!.Name,
                a.AvailabilitySlot.Availability.Doctor.Speciality?.Name ?? "",
                a.AvailabilitySlot.Date,
                a.AvailabilitySlot.StartTime.ToString("HH:mm"),
                a.AvailabilitySlot.EndTime.ToString("HH:mm"),
                a.Reason,
                a.Status.ToString()));
        }
        public async Task<Pagination<AppointmentModel.SearchResultDto>> GetByDate(DateOnly date, int pageSize, int pageIndex)
            => await Search(null, null, null, date, pageSize, pageIndex);
        public async Task<Pagination<AppointmentModel.SearchResultDto>> Search(Guid? specialtyId, Guid? doctorId, long? dni, DateOnly? date, int pageSize, int pageIndex)
        {
            var result = await _persistence.Paginate<Appointment, DateOnly>(
                pageSize, pageIndex,
                a =>
                    (specialtyId == null || a.AvailabilitySlot!.Availability!.Doctor!.SpecialityId == specialtyId) &&
                    (doctorId == null || a.AvailabilitySlot!.Availability!.DoctorId == doctorId) &&
                    (dni == null || a.Patient!.Dni == dni) &&
                    (date == null || a.AvailabilitySlot!.Date == date),
                a => a.AvailabilitySlot!.Date,
                "AvailabilitySlot.Availability.Doctor.Speciality", "Patient");
            return result.Map(a => new AppointmentModel.SearchResultDto(
        a.Id,
        a.Status.ToString(),
        new AppointmentModel.SearchPatientDto(
            a.Patient!.Dni,
            a.Patient.FullName ?? ""),                                        
        new AppointmentModel.SearchDoctorDto(
            a.AvailabilitySlot!.Availability!.DoctorId,
            a.AvailabilitySlot.Availability.Doctor!.Name,
            new AppointmentModel.SearchSpecialtyDto(
                a.AvailabilitySlot.Availability.Doctor.Speciality!.Id,
                a.AvailabilitySlot.Availability.Doctor.Speciality.Name))));
        }
        private static void Validate(AppointmentModel.Request request)
        {
            var dniLength = request.Patient.Dni.ToString().Length;
            if (request.Patient.Dni <= 0 || dniLength is < 7 or > 8)
                throw new ValidationException("El DNI debe tener entre 7 y 8 dígitos", "APPOINTMENT_DNI_INVALID");
            if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Length < 5)
                throw new ValidationException("El motivo debe tener al menos 5 caracteres", "APPOINTMENT_REASON_INVALID");
        }
    }
}
