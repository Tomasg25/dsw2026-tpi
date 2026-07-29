using Microsoft.EntityFrameworkCore;
using Dsw2026Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2026Tpi.Data.Configurations
{
    public class PatientConfiguration : IEntityTypeConfiguration<Patient>

    {
        public void Configure(EntityTypeBuilder<Patient> builder)
        {
            builder.ToTable("Patients");
            builder.HasIndex(p=>p.Dni).IsUnique();
        }
    }
}
