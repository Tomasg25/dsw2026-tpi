using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos
{
    public record AvailabilityModel
    {
        public record Request(Guid DoctorId, List<DayEntry> Days);
        public record DayEntry(string Day, string StartTime, string EndTime);
        public record WeeklyPatternDto(Guid AvailabilityId,string Day, string StartTime, string EndTime);
        public record SlotDto(Guid Id, DateOnly Date, TimeOnly StartTime, TimeOnly EndTime);


    }
}
