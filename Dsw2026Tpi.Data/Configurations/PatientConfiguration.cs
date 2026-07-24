using Microsoft.EntityFrameworkCore;
using Dsw2026Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Data.Configurations
{
    public class PatientConfiguration : IEntityTypeConfiguration<Patient>

    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Patient> builder)
        {
            builder.ToTable("Patients");
            builder.HasIndex(p=>p.Dni).IsUnique();
        }
    }
}
