using System;
using System.Collections.Generic;
using System.Text;
using NSubstitute;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.CrossCutting.Exceptions;


namespace Dsw2026Tpi.Test
{
    public class DoctorServiceTest
    {
        private readonly IPersistence _mockPersistence = Substitute.For<IPersistence>();
        private readonly ILogger<DoctorService> _mockLogger = Substitute.For<ILogger<DoctorService>>();

        [Fact]
        public async Task CreateDoctor_CuandoElNombreEsMenorA3Caracteres_EntoncesLanzaException()
        {
            // Arrange
            var service = new DoctorService(_mockPersistence, _mockLogger);

            var request = new DoctorModel.Request("Ab", "AAA-123", Guid.NewGuid());

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ValidationException>(() => service.Create(request));
            Assert.Equal("DOCTOR_NAME_INVALID", ex.Error.ErrorCode);
        }
    }
}
