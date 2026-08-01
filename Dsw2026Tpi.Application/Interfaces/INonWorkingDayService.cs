using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Interfaces
{
    public interface INonWorkingDayService
    {
        bool IsHoliday(DateOnly date);
        IReadOnlyList<(DateOnly Date, string Description)> GetAll();
    }
}
