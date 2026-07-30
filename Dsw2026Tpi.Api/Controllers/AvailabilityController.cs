using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers
{
    [Route("api/availabilities")]
    [Authorize]
    public class AvailabilityController : AppController
    {
        private readonly IAvailabilityService _service;
        public AvailabilityController(IAvailabilityService service)
        {
            _service = service;
        }
        [HttpPost]
        [Authorize(Policy = Policies.AdminPolicy)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create([FromBody] AvailabilityModel.Request request)
        {
            await _service.Create(request);
            return Ok();
        }
        [HttpPut]
        [Authorize(Policy = Policies.AdminPolicy)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromBody] AvailabilityModel.Request request)
        {
            await _service.Update(request);
            return Ok();
        }
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFreeSlots([FromQuery] Guid doctorId, [FromQuery] DateOnly? date = null)
        {
            var slots = await _service.GetFreeSlots(doctorId, date);
            return Ok(slots);
        }
    }
}
