using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public enum SlotStatus
    {
        Available,
        Booked,
        Cancelled,
        Attended,
        NoShow
    }

    public class Slot : EntityBase
    {
        public Guid AvailabilityId { get; private set; }
        public Guid DoctorId { get; private set; }
        public Availability? Availability { get; private set; }
        public Doctor? Doctor { get; private set; }
        public DateOnly Date { get; private set; }
        public TimeOnly StartTime { get; private set; }
        public TimeOnly EndTime { get; private set; }
        public SlotStatus Status { get; private set; }
        public byte[]? RowVersion { get; private set; }

        #region Constructor for EF
#pragma warning disable CS8618
        private Slot() { }
#pragma warning restore CS8618
        #endregion
        public Slot(Availability availability, Doctor doctor, DateOnly date, TimeOnly startTime, TimeOnly endTime, Guid? id = null) : base(id)
        {
            Availability = availability;
            AvailabilityId = availability.Id;
            Doctor = doctor;
            DoctorId = doctor.Id;
            Date = date;
            StartTime = startTime;
            EndTime = endTime;
            Status = SlotStatus.Available;
        }
        public void Book() => Status = SlotStatus.Booked;
        public void Free() => Status = SlotStatus.Available;
        public void MarkAttended() => Status = SlotStatus.Attended;
        public void MarkNoShow() => Status = SlotStatus.NoShow;





    }
}
