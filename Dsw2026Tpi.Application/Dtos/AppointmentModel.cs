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
        public record SearchResultDto(Guid AppointmentsId, string AppointmentsStatus, SearchPatientDto Patient, SearchDoctorDto Doctor);
        public record SearchPatientDto(long Dni, string FullName);
        public record SearchDoctorDto(Guid DoctorId, string Name, SearchSpecialtyDto Specialty);
        public record SearchSpecialtyDto(Guid SpecialtyId, string Name);
    }
}
