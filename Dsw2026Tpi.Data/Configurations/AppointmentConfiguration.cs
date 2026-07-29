using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Data.Configurations
{
    public class AppointmentConfiguration: IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.ToTable("Appointments");
            builder.Property(a => a.RowVersion).IsRowVersion();
            builder.HasIndex(a => a.SlotId).IsUnique();
            builder.HasOne(a => a.AvailabilitySlot)
                .WithMany()
                .HasForeignKey(a => a.SlotId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(a => a.Patient)
                .WithMany()
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
