using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers
{
    [Route("specialties")]
    [Authorize]
    public class SpecialityController : AppController
    {
        private readonly ISpecialityService _service;

        public SpecialityController(ISpecialityService service)
        {
            _service = service;
        }

        [HttpGet]
        //Aprender "bien" FromQuery y FromBody 
        public async Task<IActionResult> GetAll([FromQuery] int pageSize, [FromQuery] int pageIndex, [FromQuery] string? name = null)
        {
            var result = await _service.GetAll(pageSize, pageIndex, name);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = Policies.AdminPolicy)]
        public async Task<IActionResult> Create([FromBody] SpecialityModel.Request request)
        {
            var result = await _service.Create(request);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Policies.AdminPolicy)]
        public async Task<IActionResult> Update(Guid id, [FromBody] SpecialityModel.Request request)
        {
            var result = await _service.Update(id, request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Policies.AdminPolicy)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.Delete(id);
            return Ok();
        }
    }
}
