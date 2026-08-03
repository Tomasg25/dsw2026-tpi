using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Test
{
    public class SpecialityServiceTest
    {
        [Fact]
        public async Task CreateSpeciality_DescripcionMenorA10Caracteres_LanzaValidationException()
        {
            // Arrange
            var persistence = Substitute.For<IPersistence>();
            var logger = Substitute.For<ILogger<SpecialityService>>();
            var service = new SpecialityService(persistence, logger);

            var request = new SpecialityModel.Request("Cardiologia", "corta");

            // Act 
            var ex = await Assert.ThrowsAsync<ValidationException>(() => service.Create(request));

            //Assert
            Assert.Equal("SPECIALITY_DESCRIPTION_INVALID", ex.Error.ErrorCode);
        }

    }
}
