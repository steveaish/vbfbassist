using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using VBFB_API.Classes;
using VBFB_API.Models;

namespace VBFB_API.Controllers
{
    /// <summary>
    /// Cadet training items (Settings). Laravel hard-deleted them, which broke the cadet training
    /// history (inner join); items that have been used are now made inactive instead.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [RequirePermission("isadmin")]
    public class TrainingController : ControllerBase
    {
        private static readonly string[] Fillable = { "name", "resources", "Description", "IsActive" };

        private readonly ApplicationDbContext _context;

        public TrainingController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult<dynamic> List()
        {
            return Ok(_context.training.AsNoTracking().OrderBy(t => t.id).ToList());
        }

        [HttpPost]
        public ActionResult Create([FromBody] JObject data)
        {
            training item = new() { IsActive = "Y" };
            EntityUpdater.Fill(item, data, Fillable);
            if (string.IsNullOrWhiteSpace(item.name)) return BadRequest(new { message = "Please enter a name." });
            Stamp(item);
            _context.training.Add(item);
            _context.SaveChanges();
            return Ok();
        }

        [HttpPut("{id:int}")]
        public ActionResult Update(int id, [FromBody] JObject data)
        {
            training? item = _context.training.FirstOrDefault(t => t.id == id);
            if (item == null) return NotFound();
            EntityUpdater.Fill(item, data, Fillable);
            if (string.IsNullOrWhiteSpace(item.name)) return BadRequest(new { message = "Please enter a name." });
            Stamp(item);
            _context.SaveChanges();
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            training? item = _context.training.FirstOrDefault(t => t.id == id);
            if (item == null) return NotFound();

            if (_context.cadet_attendance.Any(a => a.trainingid == id))
            {
                item.IsActive = "N";
                Stamp(item);
                _context.SaveChanges();
                return Ok(new { message = "This training has been recorded against cadets, so it was made inactive instead of deleted." });
            }

            _context.training.Remove(item);
            _context.SaveChanges();
            return Ok();
        }

        private void Stamp(training item)
        {
            item.lastupdated = DateTime.Now;
            item.lastupdatedby = Permissions.CurrentProfile(HttpContext, _context)?.id;
        }
    }
}
