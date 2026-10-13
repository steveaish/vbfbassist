using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using VBFB_API.Classes;
using VBFB_API.Models;

namespace VBFB_API.Controllers
{
    /// <summary>
    /// Cadets (profiles with cadet = 'Y') and their training attendance (Laravel CadetsController).
    /// Cadet leaders can view; only admins can edit a cadet's details.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [RequirePermission("cadetleader")]
    public class CadetsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CadetsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST api/v1/cadets/opened
        [HttpPost("opened")]
        public ActionResult Opened()
        {
            ProgramLog.Write(HttpContext, _context, ProgramLog.Cadets, "Open Cadets Screen");
            return Ok();
        }

        // POST api/v1/cadets/list - Syncfusion grid; params: search, showinactive
        [HttpPost("list")]
        public ActionResult<dynamic> List([FromBody] JObject? data)
        {
            ApiBreakDown grid = ApiUtils.SyncFusionBreakDownApiBody(data, "firstname, lastname");
            string filters = ProfileFields.BuildFilters(data, grid.parameters);
            string from = $"from profiles p left join Cadet_Crews cc on p.cadetcrew = cc.seqno where p.cadet = 'Y' {filters} {grid.where}";

            var rows = SqlQuery.Select(_context,
                $@"select * from (select p.id, firstname, lastname, (isnull(firstname, '') + ' ' + isnull(lastname, '')) as fullname,
                        mobilephone, isactive, caregivername, caregivermobile, cc.CrewName {from}) x
                   order by {grid.orderby} {grid.skipandfetch}", grid.parameters);
            int count = SqlQuery.Count(_context, "select count(*) " + from, grid.parameters);

            return Ok(new { result = rows, count });
        }

        // GET api/v1/cadets/5
        [HttpGet("{id:int}")]
        public ActionResult<dynamic> Get(int id)
        {
            var rows = SqlQuery.Select(_context,
                $@"select p.id, {string.Join(", ", ProfileFields.CadetFillable.Select(f => "p." + f))}, p.mobilephone, p.isactive, cc.CrewName
                   from profiles p left join Cadet_Crews cc on p.cadetcrew = cc.seqno
                   where p.id = @id and p.cadet = 'Y'",
                new[] { new SqlParameter("@id", id) });
            if (rows.Count == 0) return NotFound();

            return Ok(new
            {
                cadet = rows[0],
                training = SqlQuery.Select(_context, @"select ca.id, ca.thedate, t.name as Training,
                        (l.firstname + ' ' + l.lastname) as UnitLeader, ca.hours
                    from cadet_attendance ca
                    inner join training t on ca.trainingid = t.id
                    left join profiles l on ca.unitleader = l.id
                    where ca.cadetid = @id
                    order by ca.thedate desc, ca.id desc", new[] { new SqlParameter("@id", id) }),
                cadetcrews = _context.Cadet_Crews.AsNoTracking().OrderBy(c => c.CrewName).Select(c => new { c.seqno, c.CrewName }).ToList(),
                documents = _context.documents.AsNoTracking().Where(d => d.doctype == "cadet" && d.sourceid == id).OrderByDescending(d => d.id)
                    .Select(d => new { d.id, d.description, d.origfilename, d.created_at }).ToList()
            });
        }

        // PUT api/v1/cadets/5
        [HttpPut("{id:int}")]
        [RequirePermission("isadmin")]
        public ActionResult Update(int id, [FromBody] JObject data)
        {
            profiles? profile = _context.profiles.FirstOrDefault(p => p.id == id && p.cadet == "Y");
            if (profile == null) return NotFound();

            EntityUpdater.Fill(profile, data, ProfileFields.CadetFillable);
            profile.updated_at = DateTime.Now;
            _context.SaveChanges();
            return Ok();
        }

        // POST api/v1/cadets/5/documents (multipart: file, description) - forms, consents etc. (Laravel /uploadfile)
        [HttpPost("{id:int}/documents")]
        [RequirePermission("isadmin")]
        [RequestSizeLimit(FileStore.MaxBytes + 1024 * 1024)]
        public async Task<ActionResult> UploadDocument(int id, [FromForm] IFormFile? file, [FromForm] string? description)
        {
            if (!_context.profiles.Any(p => p.id == id && p.cadet == "Y")) return NotFound();
            if (file == null) return BadRequest(new { message = "Please choose a file." });

            string stored;
            try { stored = await FileStore.SaveAsync(file); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }

            _context.documents.Add(new documents
            {
                doctype = "cadet",
                sourceid = id,
                description = description,
                filename = stored,
                origfilename = Path.GetFileName(file.FileName),
                created_at = DateTime.Now
            });
            _context.SaveChanges();
            return Ok();
        }

        // GET api/v1/cadets/documents/7/file?apikey=&userid=&userkey=
        [HttpGet("documents/{docId:int}/file")]
        public ActionResult DocumentFile(int docId, [FromQuery] string? download)
        {
            documents? doc = _context.documents.AsNoTracking().FirstOrDefault(d => d.id == docId && d.doctype == "cadet");
            if (doc == null) return NotFound();
            if (download != "Y") Response.Headers.ContentDisposition =
                new System.Net.Http.Headers.ContentDispositionHeaderValue("inline") { FileNameStar = doc.origfilename ?? doc.filename }.ToString();
            return DocumentsController.ServeFile(this, doc.filename, doc.origfilename, download == "Y");
        }

        // DELETE api/v1/cadets/documents/7 - removes the record (as Laravel); the file is kept on disk
        [HttpDelete("documents/{docId:int}")]
        [RequirePermission("isadmin")]
        public ActionResult DeleteDocument(int docId)
        {
            documents? doc = _context.documents.FirstOrDefault(d => d.id == docId && d.doctype == "cadet");
            if (doc == null) return NotFound();
            _context.documents.Remove(doc);
            _context.SaveChanges();
            return Ok();
        }

        // GET api/v1/cadets/trainingentry - lists for the "Record Training Event" screen
        [HttpGet("trainingentry")]
        public ActionResult<dynamic> TrainingEntry()
        {
            return Ok(new
            {
                training = _context.training.AsNoTracking().Where(t => t.IsActive == "Y").OrderBy(t => t.name).Select(t => new { t.id, t.name }).ToList(),
                unitleaders = _context.profiles.AsNoTracking().Where(p => p.cadetleader == "Y" && p.isactive == "Y")
                    .OrderBy(p => p.firstname).ThenBy(p => p.lastname).Select(p => new { p.id, name = p.firstname + " " + p.lastname }).ToList(),
                cadets = _context.profiles.AsNoTracking().Where(p => p.cadet == "Y" && p.isactive == "Y")
                    .OrderBy(p => p.firstname).ThenBy(p => p.lastname).Select(p => new { p.id, name = p.firstname + " " + p.lastname }).ToList(),
                myprofileid = Permissions.CurrentProfile(HttpContext, _context)?.id
            });
        }

        // POST api/v1/cadets/training  { thedate: yyyy-MM-dd, trainingid, unitleader, hours, cadets: [ids] }
        [HttpPost("training")]
        public ActionResult RecordTraining([FromBody] JObject data)
        {
            if (!DateTime.TryParse((string?)data["thedate"], out DateTime theDate)) return BadRequest(new { message = "Please choose a date." });
            int? trainingId = (int?)data["trainingid"];
            if (trainingId == null) return BadRequest(new { message = "Please choose the training completed." });
            if (data["cadets"] is not JArray cadets || cadets.Count == 0) return BadRequest(new { message = "Please select at least one cadet." });

            foreach (int cadetId in cadets.Select(c => (int)c).Distinct())
            {
                _context.cadet_attendance.Add(new cadet_attendance
                {
                    cadetid = cadetId,
                    trainingid = trainingId,
                    unitleader = (int?)data["unitleader"],
                    hours = (double?)data["hours"],
                    thedate = theDate.Date,
                    lastupdated = DateTime.Now
                });
            }
            _context.SaveChanges();
            return Ok(new { added = cadets.Count });
        }

        // DELETE api/v1/cadets/training/7 - remove an attendance entered by mistake
        [HttpDelete("training/{attendanceId:int}")]
        [RequirePermission("isadmin")]
        public ActionResult DeleteTraining(int attendanceId)
        {
            cadet_attendance? row = _context.cadet_attendance.FirstOrDefault(a => a.id == attendanceId);
            if (row == null) return NotFound();
            _context.cadet_attendance.Remove(row);
            _context.SaveChanges();
            return Ok();
        }
    }
}
