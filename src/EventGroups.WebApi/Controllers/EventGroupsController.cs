using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EventGroups.Contract;
using EventGroups.Storage;
using EventGroups.Storage.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventGroups.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EventGroupsController : ControllerBase
    {
        private readonly EventGroupDbContext _context;

        public EventGroupsController(EventGroupDbContext context)
        {
            _context = context;
        }

        [HttpGet("Solution/{id:guid:required}_{name:required}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Guid))]
        public async Task<ActionResult<Guid>> FindSolution(Guid id, string name)
        {
            var exist = await _context.Solutions.FirstOrDefaultAsync(s => s.Id == id && s.Name == name);

            if (exist == null)
                return NotFound();

            return exist.Id;
        }

        private static EventGroupDTO ToDto(EventGroup eventGroup)
        {
            return new EventGroupDTO()
            {
                Description = eventGroup.Description,
                EventGroupAbbr = eventGroup.Abbreviation,
                SolutionId = eventGroup.SolutionId,
                SolutionName = eventGroup.Solution.Name
            };
        }

        private static EventGroup FromDto(EventGroupDTO dto)
        {
            return new EventGroup()
            {
                Description = dto.Description,
                Abbreviation = dto.EventGroupAbbr,
                SolutionId = dto.SolutionId
            };
        }

        // GET: api/EventGroups/SolutionGroups/5
        [HttpGet("SolutionGroups/{solutionId:guid:required}")]
        public async Task<ActionResult<IEnumerable<EventGroupDTO>>> GetEventGroups(Guid solutionId)
        {
            return await _context.EventGroups.Include(e => e.Solution).Where(g => g.SolutionId == solutionId).Select(e => ToDto(e)).ToListAsync();
        }

        // GET: api/EventGroups/5
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(EventGroupDTO))]
        public async Task<ActionResult<EventGroupDTO>> GetEventGroup(Guid id)
        {
            var eventGroup = await _context.EventGroups.Include(g => g.Solution).FirstOrDefaultAsync(g => g.Oid == id);

            if (eventGroup == null)
            {
                return NotFound();
            }

            return ToDto(eventGroup);
        }

        // GET: api/EventGroups/5
        [HttpGet("Find/{solutionId:guid:required}_{abbr:required}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(EventGroupDTO))]
        public async Task<ActionResult<EventGroupDTO>> Find(Guid solutionId, string abbr)
        {
            var eventGroup = await _context.EventGroups.FirstOrDefaultAsync(g => g.Abbreviation == abbr
                && g.SolutionId == solutionId);

            if (eventGroup == null)
            {
                return NotFound();
            }

            return ToDto(eventGroup);
        }


        //// PUT: api/EventGroups/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to, for
        //// more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutEventGroup(Guid id, EventGroup eventGroup)
        //{
        //    if (id != eventGroup.Oid)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(eventGroup).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!EventGroupExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        // POST: api/EventGroups
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EventGroupDTO))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<EventGroupDTO>> PostEventGroup(EventGroupDTO eventGroup)
        {
            var solution = await _context.Solutions.FirstOrDefaultAsync(s => s.Id == eventGroup.SolutionId);            

            if (solution == null)
            {
                var owner = await _context.Users.FirstOrDefaultAsync(s => s.Id == eventGroup.Owner.ToString());
                solution = new Solution()
                {
                    Id = eventGroup.SolutionId,
                    Name = eventGroup.SolutionName,
                    Owner = owner
                };
                _context.Add(solution);
                await _context.SaveChangesAsync();
            }                        

            var exist = await _context.EventGroups.FirstOrDefaultAsync(g =>
                g.SolutionId == solution.Id && g.Abbreviation == eventGroup.EventGroupAbbr);
            if (exist != null)
                return Ok(exist);

            var eg = FromDto(eventGroup);
            eg.Solution = solution;
            _context.EventGroups.Add(eg);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEventGroup), new { id = eg.Oid }, eventGroup);
        }

        // DELETE: api/EventGroups/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(EventGroupDTO))]
        public async Task<ActionResult<EventGroupDTO>> DeleteEventGroup(Guid id)
        {
            var eventGroup = await _context.EventGroups.FindAsync(id);
            if (eventGroup == null)
            {
                return NotFound();
            }

            _context.EventGroups.Remove(eventGroup);
            await _context.SaveChangesAsync();

            return ToDto(eventGroup);
        }

        private bool EventGroupExists(Guid id)
        {
            return _context.EventGroups.Any(e => e.Oid == id);
        }

        [Authorize]
        [HttpGet("TestConnection")]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult TestConnection()
        {
            var curUserId = HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Jti);

            if (_context.Users.Any(u => u.Id == curUserId))
                return Ok();

            return Unauthorized();
        }
    }
}
