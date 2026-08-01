using Dsw2026Tpi.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Interfaces
{
    public interface IAvailabilityService
    {
        Task<IEnumerable<AvailabilityModel.WeeklyPatternDto>> Create(AvailabilityModel.Request request);
        Task<IEnumerable<AvailabilityModel.WeeklyPatternDto>> Update(AvailabilityModel.Request request);
        Task<IEnumerable<AvailabilityModel.WeeklyPatternDto>> GetWeeklyPattern(Guid doctorId); //dias disponibles del doctor
        //Task<IEnumerable<AvailabilityModel.SlotDto>> GetFreeSlots(Guid doctorId, DateOnly? date);



    }
}
