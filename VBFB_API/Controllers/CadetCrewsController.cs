using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using VBFB_API.Classes;
using VBFB_API.Models;

namespace VBFB_API.Controllers
{
    /// <summary>Cadet crews (Settings). Laravel hard-deletes these; cadets in a deleted crew show no crew.</summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [RequirePermission("isadmin")]
    public class CadetCrewsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CadetCrewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult<dynamic> List()
        {
            return Ok(_context.Cadet_Crews.AsNoTracking().OrderBy(c => c.CrewName)
                .Select(c => new { id = c.seqno, c.CrewName }).ToList());
        }

        [HttpPost]
        public ActionResult Create([FromBody] JObject data)
        {
            string? name = ((string?)data["CrewName"])?.Trim();
            if (string.IsNullOrEmpty(name)) return BadRequest(new { message = "Please enter a name." });
            _context.Cadet_Crews.Add(new Cadet_Crews { CrewName = name });
            _context.SaveChanges();
            return Ok();
        }

        [HttpPut("{id:int}")]
        public ActionResult Update(int id, [FromBody] JObject data)
        {
            Cadet_Crews? crew = _context.Cadet_Crews.FirstOrDefault(c => c.seqno == id);
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
            Cadet_Crews? crew = _context.Cadet_Crews.FirstOrDefault(c => c.seqno == id);
            if (crew == null) return NotFound();
            _context.Cadet_Crews.Remove(crew);
            _context.SaveChanges();
            return Ok();
        }
    }
}
