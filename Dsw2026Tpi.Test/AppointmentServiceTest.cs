using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Test
{
    public class AppointmentServiceTest
    {

        private readonly IPersistence _mockPersistence = Substitute.For<IPersistence>();
        private readonly ILogger<AppointmentService> _mockLogger = Substitute.For<ILogger<AppointmentService>>();



        [Fact]
        public async Task Book_CuandoEsTurnoPasado_EntoncesLanzaException()
        {
            // Arrange
            var service = new AppointmentService(_mockPersistence, _mockLogger);
            var speciality = new Speciality("Cardiologia", "Especialidad del corazon");
            var doctor = new Doctor("Juan Perez", "AAA-111", speciality, Guid.NewGuid());
            var availability = new Availability(doctor, 2025, 1, DayOfWeek.Monday,new TimeOnly(9, 0), new TimeOnly(10, 0));
            var slot = new Slot(availability, new DateOnly(2025, 1, 6),new TimeOnly(9, 0), new TimeOnly(9, 30));
            _mockPersistence.GetById<Doctor>(doctor.Id).Returns(doctor);
            _mockPersistence.GetById<Slot>(slot.Id, nameof(Slot.Availability)).Returns(slot);
            var request = new AppointmentModel.Request(doctor.Id,slot.Id, new AppointmentModel.PatientDto(33445566),"Control general de rutina");
            // Act
            var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => service.Book(request));
            // Assert 
            Assert.Equal("APPOINTMENT_PAST_DATE", ex.Error.ErrorCode);
        }
    }
}
