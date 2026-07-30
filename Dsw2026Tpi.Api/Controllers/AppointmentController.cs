using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Dsw2026Tpi.Api.Controllers
{
    [Route("api/appointment")]
    [Authorize]
    public class AppointmentController : AppController
    {
        private readonly IAppointmentService _service;
        public AppointmentController(IAppointmentService service)
        {
            _service = service;
        }
        [HttpPost]
        [Authorize(Policy = Policies.PatientPolicy)]
        public async Task<IActionResult> Book([FromBody] AppointmentModel.Request request)
        {
            var result = await _service.Book(request);
            return Ok(result);
        }
        [HttpGet("patient")]
        [Authorize(Policy = Policies.PatientPolicy)]
        public async Task<IActionResult> GetByPatient([FromQuery] long dni)
        {
            var result = await _service.GetByPatient(dni);
            return Ok(result);
        }
        [HttpDelete("{id}")]
        [Authorize(Policy = Policies.PatientPolicy)]
        public async Task<IActionResult> Cancel(Guid id)
        {
            await _service.Cancel(id);
            return Ok();
        }
        [HttpGet]
        [Authorize(Policy = Policies.AdminPolicy)]
        public async Task<IActionResult> GetByDate([FromQuery] DateOnly date, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 1)
        {
            var result = await _service.GetByDate(date, pageSize, pageIndex);
            return Ok(result);
        }
        [HttpGet("search")]
        [Authorize(Policy = Policies.AdminPolicy)]
        public async Task<IActionResult> Search([FromQuery] Guid? specialtyId, [FromQuery] Guid? doctorId, [FromQuery] long? dni, [FromQuery] DateOnly? date, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 1)
        {
            var result = await _service.Search(specialtyId, doctorId, dni, date, pageSize, pageIndex);
            return Ok(result);
        }
    }

}