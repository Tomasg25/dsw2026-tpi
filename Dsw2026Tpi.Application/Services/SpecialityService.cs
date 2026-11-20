using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.Extensions.Logging;
namespace Dsw2026Tpi.Application.Services;

public class SpecialityService : ISpecialityService
{
    private readonly IPersistence _persistence;
    private readonly ILogger<SpecialityService> _logger;
    public SpecialityService(IPersistence persistence, ILogger<SpecialityService> logger)
    {
        _persistence = persistence;
        _logger = logger;
    }
    public async Task<Pagination<SpecialityModel.Response>> GetAll(int pageSize, int pageIndex, string? name = null)
    {
        if (name != null)
        {
            bool v = name.Length is < 3 or > 100;
            if (v)
                throw new ValidationException("El nombre debe tener entre 3 y 100 caracteres", "SPECIALITY_NAME_INVALID");
        }
        var result = await _persistence.Paginate<Speciality, string>(
            pageSize, pageIndex,
            s => !s.Deleted && (string.IsNullOrWhiteSpace(name) || s.Name.Contains(name)),
            s => s.Name);
        return result.Map(s => ToResponse(s));
    }
    public async Task<SpecialityModel.Response> Create(SpecialityModel.Request request)
    {
        Validate(request);
        var speciality = new Speciality(request.Name, request.Description);
        await _persistence.Add(speciality);
        return ToResponse(speciality);
    }
    public async Task<SpecialityModel.Response> Update(Guid id, SpecialityModel.Request request)
    {
        Validate(request);
        var speciality = await _persistence.GetById<Speciality>(id)
            ?? throw new EntityNotFoundException(nameof(Speciality));
        speciality.UpdateDetails(request.Name, request.Description);
        await _persistence.Update(speciality);
        return ToResponse(speciality);
    }
    public async Task Delete(Guid id)
    {
        var speciality = await _persistence.GetById<Speciality>(id)
            ?? throw new EntityNotFoundException(nameof(Speciality));
        var hasActiveDoctors = await _persistence.First<Doctor>(d =>
            d.SpecialityId == id && !d.Deleted);
        if (hasActiveDoctors is not null)
            throw new BusinessRuleException(
                "No se puede eliminar una especialidad que tiene doctores asignados",
                "SPECIALITY_HAS_DOCTORS");
        var hasBookedAppointments = await _persistence.First<Appointment>(a =>
            a.AvailabilitySlot!.Availability!.Doctor!.SpecialityId == id &&
            a.Status == AppointmentStatus.Booked,
            "AvailabilitySlot.Availability.Doctor");
        if (hasBookedAppointments is not null)
            throw new BusinessRuleException(
                "No se puede eliminar una especialidad que tiene turnos reservados",
                "SPECIALITY_HAS_BOOKED_APPOINTMENTS");
        speciality.IsDelete();
        await _persistence.Update(speciality);
    }

    public async Task<SpecialityModel.Response> ToggleStatus(Guid id)
    {
        var speciality = await _persistence.GetById<Speciality>(id)
            ?? throw new EntityNotFoundException(nameof(Speciality));
        if (speciality.IsActive)
        {
            var hasActiveDoctors = await _persistence.First<Doctor>(d =>
                d.SpecialityId == id && !d.Deleted);
            if (hasActiveDoctors is not null)
                throw new BusinessRuleException(
                    "No se puede desactivar una especialidad que tiene doctores asignados",
                    "SPECIALITY_HAS_DOCTORS");
            var hasBookedAppointments = await _persistence.First<Appointment>(a =>
                a.AvailabilitySlot!.Availability!.Doctor!.SpecialityId == id &&
                a.Status == AppointmentStatus.Booked,
                "AvailabilitySlot.Availability.Doctor");
            if (hasBookedAppointments is not null)
                throw new BusinessRuleException(
                    "No se puede desactivar una especialidad que tiene turnos reservados",
                    "SPECIALITY_HAS_BOOKED_APPOINTMENTS");
            speciality.Deactivate();
        }
        else
        {
            speciality.Activate();
        }
        await _persistence.Update(speciality);
        return ToResponse(speciality);
    }

    private static SpecialityModel.Response ToResponse(Speciality s) =>
        new(s.Id, s.Name, s.Description, s.IsActive);
    private static void Validate(SpecialityModel.Request request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length is < 3 or > 100)
            throw new ValidationException("El nombre debe tener entre 3 y 100 caracteres", "SPECIALITY_NAME_INVALID");
        if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length is < 10 or > 100)
            throw new ValidationException("La descripción debe tener entre 10 y 100 caracteres", "SPECIALITY_DESCRIPTION_INVALID");
    }
}
