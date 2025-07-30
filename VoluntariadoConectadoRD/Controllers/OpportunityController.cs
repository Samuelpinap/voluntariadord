using Microsoft.AspNetCore.Mvc;
using VoluntariadoConectadoRD.Interfaces;
using VoluntariadoConectadoRD.Models.DTOs;
using VoluntariadoConectadoRD.Services;

namespace VoluntariadoConectadoRD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OpportunityControllerTest : ControllerBase
    {
        private readonly IOpportunityService _opportunityService;

        public OpportunityControllerTest(IOpportunityService opportunityService)
        {
            _opportunityService = opportunityService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReadOpportunityTestDto>>> GetAll()
        {
            var opportunities = await _opportunityService.GetAllOpportunitiesAsync();
            return Ok(opportunities);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReadOpportunityTestDto>> GetById(int id)
        {
            var opportunity = await _opportunityService.GetOpportunitysByIdAsync(id);

            if (opportunity == null)
                return NotFound();

            return Ok(opportunity);
        }

        [HttpPost("{organizationId}")]
        public async Task<ActionResult<ReadOpportunityTestDto>> Create(int organizationId, [FromBody] CreateOpportunityTestDto createDto)
        {
            var newOpportunity = await _opportunityService.CreateOpportunityAsync(createDto, organizationId);
            return CreatedAtAction(nameof(GetById), new { id = newOpportunity.Id }, newOpportunity);
        }

        [HttpPut("{id}/organization/{organizationId}")]
        public async Task<ActionResult<ReadOpportunityTestDto>> Update(int id, int organizationId, [FromBody] UpdateOpportunityTestDto updateDto)
        {
            var updated = await _opportunityService.UpdateOpportunityAsync(id, updateDto, organizationId);

            if (updated == null)
                return NotFound();

            return Ok(updated);
        }

        [HttpDelete("{id}/organization/{organizationId}")]
        public async Task<IActionResult> Delete(int id, int organizationId)
        {
            var deleted = await _opportunityService.DeleteOpportunityAsync(id, organizationId);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}
