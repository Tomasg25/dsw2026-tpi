using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public enum SlotStatus
    {
        Available,
        Booked,
        Blocked,
    }

    public class Slot : EntityDeletable
    {
        public Guid AvailabilityId { get; private set; }
        public Availability? Availability { get; private set; }
        public DateOnly Date { get; private set; }
        public TimeOnly StartTime { get; private set; }
        public TimeOnly EndTime { get; private set; }
        public SlotStatus Status { get; private set; }
        //public byte[]? RowVersion { get; private set; }

        #region Constructor for EF
#pragma warning disable CS8618
        private Slot() { }
#pragma warning restore CS8618
        #endregion
        public Slot(Availability availability, DateOnly date, TimeOnly startTime, TimeOnly endTime, Guid? id = null) : base(id)
        {
            Availability = availability;
            AvailabilityId = availability.Id;
            Date = date;
            StartTime = startTime;
            EndTime = endTime;
            Status = SlotStatus.Available;
        }
        public void Book() => Status = SlotStatus.Booked;
        public void Free() => Status = SlotStatus.Available;

        public void Block() => Status = SlotStatus.Blocked;
    }
}
