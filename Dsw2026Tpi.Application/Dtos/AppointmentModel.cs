using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos
{
    public record AppointmentModel
    {
        public record Request(Guid DoctorId, Guid AvailabilitySlotId, PatientDto Patient, string Reason);
        public record PatientDto(long Dni);
        public record Response(Guid Id, string Status);
        public record PatientAppointmentDto(Guid Id, string Doctor, string Speciality, DateOnly Date, string StartTime, string EndTime, string Reason, string Status);
        public record SearchResultDto(Guid Id, string Specialty, string Doctor, DateOnly Date, string AvailableTime, string Status);
    }
}
