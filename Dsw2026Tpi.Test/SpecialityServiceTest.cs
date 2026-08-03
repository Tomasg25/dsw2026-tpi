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
        private readonly IPersistence _mockPersistence = Substitute.For<IPersistence>();
        private readonly ILogger<SpecialityService> _mocklogger = Substitute.For<ILogger<SpecialityService>>();
     
        [Fact]
        public async Task CreateSpeciality_DescripcionMenorA10Caracteres_LanzaValidationException()
        {
            // Arrange
            
           
            var service = new SpecialityService(_mockPersistence, _mocklogger);

            var request = new SpecialityModel.Request("Cardiologia", "corta");

            // Act 
            var ex = await Assert.ThrowsAsync<ValidationException>(() => service.Create(request));

            //Assert
            Assert.Equal("SPECIALITY_DESCRIPTION_INVALID", ex.Error.ErrorCode);
        }

        [Fact]
        public async Task CreateSpeciality_NombreMayorA100Caracteres_LanzaValidationException()
        {
            // Arrange
          
            var service = new SpecialityService(_mockPersistence, _mocklogger);
            var nombreLargo = new string('A', 101);
            var request = new SpecialityModel.Request(nombreLargo, "Descripcion valida de prueba");
            // Act 
            var ex = await Assert.ThrowsAsync<ValidationException>(() => service.Create(request));
            //Assert
            Assert.Equal("SPECIALITY_NAME_INVALID", ex.Error.ErrorCode);
        }
    }
}

