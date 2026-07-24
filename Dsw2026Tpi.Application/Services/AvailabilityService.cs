using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Services
{


    public class AvailabilityService : IAvailabilityService
    {
        private static readonly Dictionary<string, DayOfWeek> DayNames = new(StringComparer.OrdinalIgnoreCase)
        {
            ["LUNES"] = DayOfWeek.Monday,
            ["MARTES"] = DayOfWeek.Tuesday,
            ["MIERCOLES"] = DayOfWeek.Wednesday,
            ["JUEVES"] = DayOfWeek.Thursday,
            ["VIERNES"] = DayOfWeek.Friday,
            ["SABADO"] = DayOfWeek.Saturday,
            ["DOMINGO"] = DayOfWeek.Sunday
        };
        private static readonly Dictionary<DayOfWeek, string> DayOfWeekNames = new()
        {
            [DayOfWeek.Monday] = "LUNES",
            [DayOfWeek.Tuesday] = "MARTES",
            [DayOfWeek.Wednesday] = "MIERCOLES",
            [DayOfWeek.Thursday] = "JUEVES",
            [DayOfWeek.Friday] = "VIERNES",
            [DayOfWeek.Saturday] = "SABADO",
            [DayOfWeek.Sunday] = "DOMINGO"
        };
        private readonly IPersistence _persistence;
        public AvailabilityService(IPersistence persistence)
        {
            _persistence = persistence;
        }
        public async Task Create(AvailabilityModel.Request request) => await GenerateInternal(request, overwrite: false);
        public async Task Update(AvailabilityModel.Request request) => await GenerateInternal(request, overwrite: true);
        public async Task<IEnumerable<AvailabilityModel.WeeklyPatternDto>> GetWeeklyPattern(Guid doctorId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var availabilities = await _persistence.GetFiltered<Availability>(a =>
                a.DoctorId == doctorId && a.Year == today.Year && a.Month == today.Month) ?? [];
            return availabilities.Select(a => new AvailabilityModel.WeeklyPatternDto(
                DayOfWeekNames[a.DayOfWeek], a.StartTime.ToString("HH:mm"), a.EndTime.ToString("HH:mm")));
        }
        public async Task<IEnumerable<AvailabilityModel.SlotDto>> GetFreeSlots(Guid doctorId, DateOnly? date)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var slots = await _persistence.GetFiltered<Slot>(s =>
                s.DoctorId == doctorId &&
                s.Status == SlotStatus.Available &&
                s.Date >= today &&
                (date == null || s.Date == date)) ?? [];
            return slots.OrderBy(s => s.Date).ThenBy(s => s.StartTime)
                .Select(s => new AvailabilityModel.SlotDto(s.Id, s.Date, s.StartTime, s.EndTime));
        }
        private async Task GenerateInternal(AvailabilityModel.Request request, bool overwrite)
        {
            var doctor = await _persistence.GetById<Doctor>(request.DoctorId)
                ?? throw new EntityNotFoundException(nameof(Doctor));
            Validate(request);
            var today = DateOnly.FromDateTime(DateTime.Today);
            var year = today.Year;
            var month = today.Month;
            if (overwrite)
                await RemoveExistingUnbookedSchedule(request.DoctorId, year, month);
            foreach (var entry in request.Days)
            {
                var dayOfWeek = DayNames[entry.Day];
                var startTime = TimeOnly.Parse(entry.StartTime);
                var endTime = TimeOnly.Parse(entry.EndTime);
                if (!overwrite)
                {
                    var alreadyExists = await _persistence.First<Availability>(a =>
                        a.DoctorId == request.DoctorId && a.Year == year && a.Month == month && a.DayOfWeek == dayOfWeek);
                    if (alreadyExists is not null) continue; // ya configurado este mes, no duplicar
                }
                var availability = new Availability(doctor, year, month, dayOfWeek, startTime, endTime);
                await _persistence.Add(availability);
                foreach (var date in DatesInMonthFor(dayOfWeek, today))
                {
                    for (var slotStart = startTime; slotStart < endTime; slotStart = slotStart.AddMinutes(30))
                    {
                        var slot = new Slot(availability, doctor, date, slotStart, slotStart.AddMinutes(30));
                        await _persistence.Add(slot);
                    }
                }
            }
        }
        private async Task RemoveExistingUnbookedSchedule(Guid doctorId, int year, int month)
        {
            var existingAvailabilities = await _persistence.GetFiltered<Availability>(a =>
                a.DoctorId == doctorId && a.Year == year && a.Month == month) ?? [];
            foreach (var availability in existingAvailabilities)
            {
                var slots = (await _persistence.GetFiltered<Slot>(s => s.AvailabilityId == availability.Id) ?? []).ToList();
                if (slots.Any(s => s.Status != SlotStatus.Available))
                    continue; // tiene turnos reservados/atendidos -> se preserva, no se toca
                foreach (var slot in slots)
                    await _persistence.Delete(slot);
                await _persistence.Delete(availability);
            }
        }
        private static IEnumerable<DateOnly> DatesInMonthFor(DayOfWeek dayOfWeek, DateOnly from)
        {
            var end = new DateOnly(from.Year, from.Month, DateTime.DaysInMonth(from.Year, from.Month));
            for (var date = from; date <= end; date = date.AddDays(1))
            {
                if (date.DayOfWeek == dayOfWeek)
                    yield return date;
            }
        }
        private static void Validate(AvailabilityModel.Request request)
        {
            if (request.Days is null || request.Days.Count == 0)
                throw new ValidationException("Debe indicar al menos un día", "AVAILABILITY_DAYS_REQUIRED");
            if (request.Days.Select(d => d.Day.ToUpperInvariant()).Distinct().Count() != request.Days.Count)
                throw new ValidationException("No se puede repetir el mismo día en la misma solicitud", "AVAILABILITY_DUPLICATE_DAY");
            foreach (var entry in request.Days)
            {
                if (!DayNames.ContainsKey(entry.Day))
                    throw new ValidationException($"Día inválido: {entry.Day}", "AVAILABILITY_INVALID_DAY");
                if (!TimeOnly.TryParse(entry.StartTime, out var start) || !TimeOnly.TryParse(entry.EndTime, out var end) || start >= end)
                    throw new ValidationException("El horario de inicio debe ser menor al de salida", "AVAILABILITY_INVALID_RANGE");
            }
        }
    }





}
