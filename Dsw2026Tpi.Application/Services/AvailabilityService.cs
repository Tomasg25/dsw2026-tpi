using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.Extensions.Logging;
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
            ["MIÉRCOLES"] = DayOfWeek.Wednesday,
            ["MIERCOLES"] = DayOfWeek.Wednesday,
            ["JUEVES"] = DayOfWeek.Thursday,
            ["VIERNES"] = DayOfWeek.Friday,
            ["SÁBADO"] = DayOfWeek.Saturday,
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
        private readonly INonWorkingDayService _nonWorkingDayService;
        private readonly ILogger<AvailabilityService> _logger;
        public AvailabilityService(IPersistence persistence, INonWorkingDayService nonWorkingDayService, ILogger<AvailabilityService> logger)
        {
            _persistence = persistence;
            _nonWorkingDayService = nonWorkingDayService;
            _logger = logger;
        }
        public async Task<IEnumerable<AvailabilityModel.WeeklyPatternDto>> Create(AvailabilityModel.Request request) => await GenerateInternal(request, overwrite: false);
        public async Task<IEnumerable<AvailabilityModel.WeeklyPatternDto>> Update(AvailabilityModel.Request request) => await GenerateInternal(request, overwrite: true);
        public async Task<IEnumerable<AvailabilityModel.WeeklyPatternDto>> GetWeeklyPattern(Guid doctorId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var availabilities = (await _persistence.GetFiltered<Availability>(a =>
                a.DoctorId == doctorId && a.Year == today.Year && a.Month == today.Month) ?? []).ToList();

            var result = new List<AvailabilityModel.WeeklyPatternDto>();
            foreach (var a in availabilities)
            {
                var slots = (await _persistence.GetFiltered<Slot>(s => s.AvailabilityId == a.Id) ?? []).ToList();
                var hasBookings = slots.Any(s => s.Status != SlotStatus.Available);
                result.Add(new AvailabilityModel.WeeklyPatternDto(a.Id,
                    DayOfWeekNames[a.DayOfWeek], a.StartTime.ToString("HH:mm"), a.EndTime.ToString("HH:mm"), hasBookings));
            }
            return result;
        }
        public async Task<IEnumerable<AvailabilityModel.SlotDto>> GetFreeSlots(Guid doctorId, DateOnly? date)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var slots = await _persistence.GetFiltered<Slot>(s =>
                s.Availability.DoctorId == doctorId &&
                s.Status == SlotStatus.Available &&
                s.Date >= today &&
                (date == null || s.Date == date)) ?? [];
            return slots.OrderBy(s => s.Date).ThenBy(s => s.StartTime)
                .Select(s => new AvailabilityModel.SlotDto(s.Id, s.Date, s.StartTime, s.EndTime));
        }
        private async Task<IEnumerable<AvailabilityModel.WeeklyPatternDto>> GenerateInternal(AvailabilityModel.Request request, bool overwrite)
        {
            var doctor = await _persistence.GetById<Doctor>(request.DoctorId)
                ?? throw new EntityNotFoundException(nameof(Doctor));
            Validate(request);
            var today = DateOnly.FromDateTime(DateTime.Today);
            var year = today.Year;
            var month = today.Month;

            if (!overwrite)
                await ValidateNoOverlap(request, year, month);

            var preserved = new HashSet<(DayOfWeek Day, TimeOnly Start, TimeOnly End)>();
            if (overwrite)
            {
                var requestedEntries = request.Days
                    .Select(d => (DayNames[d.Day], TimeOnly.Parse(d.StartTime), TimeOnly.Parse(d.EndTime)))
                    .ToHashSet();
                var existing = (await _persistence.GetFiltered<Availability>(a =>
                    a.DoctorId == request.DoctorId && a.Year == year && a.Month == month) ?? []).ToList();

                foreach (var avail in existing)
                {
                    if (!requestedEntries.Contains((avail.DayOfWeek, avail.StartTime, avail.EndTime)))
                    {
                        var slots = (await _persistence.GetFiltered<Slot>(s =>
                            s.AvailabilityId == avail.Id) ?? []).ToList();
                        if (slots.Any(s => s.Status != SlotStatus.Available))
                            throw new BusinessRuleException(
                                "No se puede eliminar una disponibilidad con turnos asignados",
                                "AVAILABILITY_HAS_BOOKINGS");
                    }
                }

                preserved = await RemoveExistingUnbookedSchedule(request.DoctorId, year, month);
            }

            var created = new List<Availability>();

            foreach (var entry in request.Days)
            {
                var dayOfWeek = DayNames[entry.Day];
                var startTime = TimeOnly.Parse(entry.StartTime);
                var endTime = TimeOnly.Parse(entry.EndTime);

                if (preserved.Contains((dayOfWeek, startTime, endTime)))
                    continue;

                foreach (var p in preserved.Where(p => p.Day == dayOfWeek))
                {
                    if (startTime < p.End && p.Start < endTime)
                        throw new BusinessRuleException(
                            "El horario se superpone con una disponibilidad que tiene turnos asignados",
                            "AVAILABILITY_OVERLAP_BOOKED");
                }

                var availability = new Availability(doctor, year, month, dayOfWeek, startTime, endTime);
                await _persistence.Add(availability);

                created.Add(availability);

                foreach (var date in DatesInMonthFor(dayOfWeek, today))
                {
                    if (_nonWorkingDayService.IsHoliday(date))
                        continue;

                    for (var slotStart = startTime; slotStart < endTime; slotStart = slotStart.AddMinutes(30))
                    {
                        var slot = new Slot(availability, date, slotStart, slotStart.AddMinutes(30));
                        await _persistence.Add(slot);
                    }
                }
            }

            _logger.LogInformation("Disponibilidad {Action} para DoctorId={DoctorId}, Mes={Month}/{Year}",
                overwrite ? "actualizada" : "creada", request.DoctorId, month, year);

            return created.Select(a => new AvailabilityModel.WeeklyPatternDto(a.DoctorId,
                DayOfWeekNames[a.DayOfWeek], a.StartTime.ToString("HH:mm"), a.EndTime.ToString("HH:mm")));
        }
        private async Task ValidateNoOverlap(AvailabilityModel.Request request, int year, int month)
        {
            foreach (var entry in request.Days)
            {
                var dayOfWeek = DayNames[entry.Day];
                var startTime = TimeOnly.Parse(entry.StartTime);
                var endTime = TimeOnly.Parse(entry.EndTime);

                var existing = await _persistence.GetFiltered<Availability>(a =>
                    a.DoctorId == request.DoctorId && a.Year == year && a.Month == month && a.DayOfWeek == dayOfWeek) ?? [];

                foreach (var ex in existing)
                {
                    if (startTime < ex.EndTime && endTime > ex.StartTime)
                        throw new BusinessRuleException(
                            "El horario se solapa con una disponibilidad existente para el mismo día",
                            "AVAILABILITY_OVERLAP");
                }
            }
        }
        private async Task<HashSet<(DayOfWeek Day, TimeOnly Start, TimeOnly End)>> RemoveExistingUnbookedSchedule(Guid doctorId, int year, int month)
        {
            var existingAvailabilities = (await _persistence.GetFiltered<Availability>(a =>
                a.DoctorId == doctorId && a.Year == year && a.Month == month) ?? []).ToList();

            var preserved = new HashSet<(DayOfWeek Day, TimeOnly Start, TimeOnly End)>();
            var toDelete = new List<(Availability avail, List<Slot> slots)>();

            foreach (var availability in existingAvailabilities)
            {
                var slots = (await _persistence.GetFiltered<Slot>(s =>
                    s.AvailabilityId == availability.Id) ?? []).ToList();

                if (slots.Any(s => s.Status != SlotStatus.Available))
                {
                    preserved.Add((availability.DayOfWeek, availability.StartTime, availability.EndTime));
                    continue;
                }

                toDelete.Add((availability, slots));
            }

            foreach (var (avail, slots) in toDelete)
            {
                foreach (var slot in slots)
                    await _persistence.Delete(slot);
                await _persistence.Delete(avail);
            }

            return preserved;
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
            foreach (var entry in request.Days)
            {
                if (!DayNames.ContainsKey(entry.Day))
                    throw new ValidationException($"Día inválido: {entry.Day}", "AVAILABILITY_INVALID_DAY");
                if (!TimeOnly.TryParse(entry.StartTime, out var start) || !TimeOnly.TryParse(entry.EndTime, out var end) || start >= end)
                    throw new ValidationException("El horario de inicio debe ser menor al de salida", "AVAILABILITY_INVALID_RANGE");
            }
            var grouped = request.Days.GroupBy(d => d.Day.ToUpperInvariant());
            foreach (var group in grouped)
            {
                var entries = group.ToList();
                for (int i = 0; i < entries.Count; i++)
                {
                    var startA = TimeOnly.Parse(entries[i].StartTime);
                    var endA = TimeOnly.Parse(entries[i].EndTime);
                    for (int j = i + 1; j < entries.Count; j++)
                    {
                        var startB = TimeOnly.Parse(entries[j].StartTime);
                        var endB = TimeOnly.Parse(entries[j].EndTime);
                        if (startA < endB && startB < endA)
                            throw new ValidationException(
                                $"Los horarios del día {group.Key} se superponen",
                                "AVAILABILITY_TIME_OVERLAP");
                    }
                }
            }
        }
    }
}