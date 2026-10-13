using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using VBFB_API.Classes;
using VBFB_API.Models;

namespace VBFB_API.Controllers
{
    /// <summary>Member crews (Settings). Deleting only flags the crew, as profiles still point at it.</summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [RequirePermission("isadmin")]
    public class CrewsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CrewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult<dynamic> List()
        {
            return Ok(_context.Crews.AsNoTracking().Where(c => c.deleted != "Y").OrderBy(c => c.CrewName)
                .Select(c => new { id = c.seqno, c.CrewName }).ToList());
        }

        [HttpPost]
        public ActionResult Create([FromBody] JObject data)
        {
            string? name = ((string?)data["CrewName"])?.Trim();
            if (string.IsNullOrEmpty(name)) return BadRequest(new { message = "Please enter a name." });
            _context.Crews.Add(new Crews { CrewName = name, deleted = "N" });
            _context.SaveChanges();
            return Ok();
        }

        [HttpPut("{id:int}")]
        public ActionResult Update(int id, [FromBody] JObject data)
        {
            Crews? crew = _context.Crews.FirstOrDefault(c => c.seqno == id);
            if (crew == null) return NotFound();
            string? name = ((string?)data["CrewName"])?.Trim();
            if (string.IsNullOrEmpty(name)) return BadRequest(new { message = "Please enter a name." });
            crew.CrewName = name;
            _context.SaveChanges();
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            Crews? crew = _context.Crews.FirstOrDefault(c => c.seqno == id);
            if (crew == null) return NotFound();
            crew.deleted = "Y";
            _context.SaveChanges();
            return Ok();
        }
    }
}
