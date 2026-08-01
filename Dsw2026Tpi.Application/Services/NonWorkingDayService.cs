using Dsw2026Tpi.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Dsw2026Tpi.Application.Services
{
    public class NonWorkingDayService : INonWorkingDayService
    {
        private readonly HashSet<DateOnly> _holidays;
        private readonly List<(DateOnly Date, string Description)> _holidayList;
        private readonly ILogger<NonWorkingDayService> _logger;

        public NonWorkingDayService(string jsonFilePath, ILogger<NonWorkingDayService> logger)
        {
            _holidays = [];
            _holidayList = [];
            _logger = logger;

            if (!File.Exists(jsonFilePath))
            {
                logger.LogWarning("Archivo de feriados no encontrado en {Path}", jsonFilePath);
                return;
            }

            var json = File.ReadAllText(jsonFilePath);
            var entries = JsonSerializer.Deserialize<List<HolidayEntry>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (entries is null) return;

            foreach (var entry in entries)
            {
                if (DateOnly.TryParse(entry.Fecha, out var date))
                {
                    _holidays.Add(date);
                    _holidayList.Add((date, entry.Descripcion ?? ""));
                }
            }

            logger.LogInformation("Se cargaron {Count} feriados desde {Path}", _holidays.Count, jsonFilePath);
        }

        public bool IsHoliday(DateOnly date) => _holidays.Contains(date);

        public IReadOnlyList<(DateOnly Date, string Description)> GetAll() => _holidayList;

        private sealed class HolidayEntry
        {
            public string Fecha { get; set; } = "";
            public string? Descripcion { get; set; }
        }
    }
}
