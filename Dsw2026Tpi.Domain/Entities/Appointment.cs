using System;

namespace Dsw2026Tpi.Domain.Entities;

public enum AppointmentStatus
{
    Booked,
    Cancelled,
    Attended,
    NoShow
}

public class Appointment : EntityBase
{
    public Guid SlotId { get; private set; }
    public Guid PatientId { get; private set; }
    public Slot? AvailabilitySlot { get; private set; }
    public Patient? Patient { get; private set; }
    public string Reason { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public DateTime? AttendedAt { get; private set; }

    // Campo para manejar la concurrencia (evitar doble reserva)
    public byte[]? RowVersion { get; private set; }

    #region Constructor for EF
#pragma warning disable CS8618
    private Appointment() { }
#pragma warning restore CS8618
    #endregion

    public Appointment(Guid slotId, Guid patientId, string reason, Guid? id = null) : base(id)
    {
        SlotId = slotId;
        PatientId = patientId;
        Reason = reason;
        Status = AppointmentStatus.Booked;
    }

    // Métodos para encapsular la lógica de cambio de estado de la cita
    public void Cancel()
    {
        Status = AppointmentStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Attended()
    {
        Status = AppointmentStatus.Attended;
        AttendedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void NoShow()
    {
        Status = AppointmentStatus.NoShow;
        UpdatedAt = DateTime.UtcNow;
    }
}