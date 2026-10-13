using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VBFB_API.Classes;
using VBFB_API.Models;

namespace VBFB_API.Controllers
{
    /// <summary>
    /// Brigade documents library (Laravel DocumentsController): DocumentMaster holds the current version,
    /// DocumentHistory the replaced ones. Everyone can read; Can Edit Documents can upload/change/delete.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        public static readonly string[] Types = { "Not Specified", "Meeting Minutes", "SOP", "Training" };

        private readonly ApplicationDbContext _context;

        public DocumentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("opened")]
        public ActionResult Opened()
        {
            ProgramLog.Write(HttpContext, _context, ProgramLog.Documents, "Open Documents Screen");
            return Ok();
        }

        // GET api/v1/documents?search=
        [HttpGet]
        public ActionResult<dynamic> List([FromQuery] string? search)
        {
            List<SqlParameter> ps = new();
            string where = "";
            if (!string.IsNullOrWhiteSpace(search))
            {
                ps.Add(new SqlParameter("@search", search.Trim()));
                where = "and (documentname like '%' + @search + '%' or description like '%' + @search + '%' or Author like '%' + @search + '%' or Type like '%' + @search + '%')";
            }

            return Ok(new
            {
                documents = SqlQuery.Select(_context, $@"select seqno, documentname, description, Type, Author, versionnumber, originalfilename,
                        createddate, lastupdated
                    from DocumentMaster where deleted = 'N' {where}
                    order by createddate desc", ps),
                types = Types,
                canedit = Permissions.Has(HttpContext, _context, "caneditdocuments")
            });
        }

        // GET api/v1/documents/5 - with its version history
        [HttpGet("{id:int}")]
        public ActionResult<dynamic> Get(int id)
        {
            DocumentMaster? doc = _context.DocumentMaster.AsNoTracking().FirstOrDefault(d => d.seqno == id && d.deleted == "N");
            if (doc == null) return NotFound();

            return Ok(new
            {
                document = new { doc.seqno, doc.documentname, doc.description, doc.Type, doc.Author, doc.versionnumber, doc.originalfilename, doc.createddate, doc.lastupdated },
                history = _context.DocumentHistory.AsNoTracking().Where(h => h.originalseqno == id).OrderByDescending(h => h.versionnumber)
                    .Select(h => new { h.seqno, h.versionnumber, h.originalfilename, h.createddate, h.lastupdated }).ToList()
            });
        }

        // POST api/v1/documents (multipart: file, documentname, description, Type, Author)
        [HttpPost]
        [RequirePermission("caneditdocuments")]
        [RequestSizeLimit(FileStore.MaxBytes + 1024 * 1024)]
        public async Task<ActionResult<dynamic>> Upload([FromForm] IFormFile? file, [FromForm] string? documentname, [FromForm] string? description,
            [FromForm] string? Type, [FromForm] string? Author)
        {
            if (file == null) return BadRequest(new { message = "Please choose a file." });
            if (string.IsNullOrWhiteSpace(documentname)) return BadRequest(new { message = "Please enter a document name." });

            string stored;
            try { stored = await FileStore.SaveAsync(file); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }

            DocumentMaster doc = new()
            {
                documentname = documentname.Trim(),
                description = description,
                Type = Types.Contains(Type) ? Type : "Not Specified",
                Author = Author,
                filename = stored,
                originalfilename = Path.GetFileName(file.FileName),
                versionnumber = 1,
                createddate = DateTime.Now,
                lastupdated = DateTime.Now,
                uploadedby = Permissions.CurrentProfile(HttpContext, _context)?.id,
                deleted = "N"
            };
            _context.DocumentMaster.Add(doc);
            _context.SaveChanges();
            return Ok(new { id = doc.seqno });
        }

        // PUT api/v1/documents/5 { documentname, description, Type, Author }
        [HttpPut("{id:int}")]
        [RequirePermission("caneditdocuments")]
        public ActionResult Update(int id, [FromBody] Newtonsoft.Json.Linq.JObject data)
        {
            DocumentMaster? doc = _context.DocumentMaster.FirstOrDefault(d => d.seqno == id && d.deleted == "N");
            if (doc == null) return NotFound();
            string? name = ((string?)data["documentname"])?.Trim();
            if (string.IsNullOrEmpty(name)) return BadRequest(new { message = "Please enter a document name." });

            doc.documentname = name;
            doc.description = (string?)data["description"];
            string? type = (string?)data["Type"];
            doc.Type = Types.Contains(type) ? type : doc.Type;
            doc.Author = (string?)data["Author"];
            doc.lastupdated = DateTime.Now;
            _context.SaveChanges();
            return Ok();
        }

        // POST api/v1/documents/5/file (multipart: file) - new version; the old one moves to DocumentHistory
        [HttpPost("{id:int}/file")]
        [RequirePermission("caneditdocuments")]
        [RequestSizeLimit(FileStore.MaxBytes + 1024 * 1024)]
        public async Task<ActionResult> NewVersion(int id, [FromForm] IFormFile? file)
        {
            DocumentMaster? doc = _context.DocumentMaster.FirstOrDefault(d => d.seqno == id && d.deleted == "N");
            if (doc == null) return NotFound();
            if (file == null) return BadRequest(new { message = "Please choose a file." });

            string stored;
            try { stored = await FileStore.SaveAsync(file); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }

            _context.DocumentHistory.Add(new DocumentHistory
            {
                originalseqno = id,
                description = doc.description,
                createddate = doc.createddate,
                lastupdated = doc.lastupdated,
                documentname = doc.documentname,
                filename = doc.filename,
                versionnumber = doc.versionnumber,
                originalfilename = doc.originalfilename,
                uploadedby = doc.uploadedby
            });

            doc.filename = stored;
            doc.originalfilename = Path.GetFileName(file.FileName);
            doc.versionnumber = (doc.versionnumber ?? 1) + 1;
            doc.lastupdated = DateTime.Now;
            doc.uploadedby = Permissions.CurrentProfile(HttpContext, _context)?.id;
            _context.SaveChanges();
            return Ok();
        }

        // DELETE api/v1/documents/5 - soft delete, as Laravel
        [HttpDelete("{id:int}")]
        [RequirePermission("caneditdocuments")]
        public ActionResult Delete(int id)
        {
            DocumentMaster? doc = _context.DocumentMaster.FirstOrDefault(d => d.seqno == id);
            if (doc == null) return NotFound();
            doc.deleted = "Y";
            _context.SaveChanges();
            return Ok();
        }

        // GET api/v1/documents/5/file?apikey=&userid=&userkey=[&download=Y] - opened in a new tab
        [HttpGet("{id:int}/file")]
        public ActionResult File(int id, [FromQuery] string? download)
        {
            DocumentMaster? doc = _context.DocumentMaster.AsNoTracking().FirstOrDefault(d => d.seqno == id && d.deleted == "N");
            return doc == null ? NotFound() : Serve(doc.filename, doc.originalfilename, download == "Y");
        }

        // GET api/v1/documents/history/3/file - an older version
        [HttpGet("history/{historyId:int}/file")]
        public ActionResult HistoryFile(int historyId, [FromQuery] string? download)
        {
            DocumentHistory? h = _context.DocumentHistory.AsNoTracking().FirstOrDefault(d => d.seqno == historyId);
            return h == null ? NotFound() : Serve(h.filename, h.originalfilename, download == "Y");
        }

        /// <summary>Streams a stored upload inline (or as a download) under its original name.</summary>
        public static ActionResult ServeFile(ControllerBase controller, string? storedName, string? originalName, bool download)
        {
            string? path = FileStore.PathFor(storedName);
            if (path == null) return controller.NotFound(new { message = "The file could not be found." });

            string name = string.IsNullOrWhiteSpace(originalName) ? storedName! : originalName;
            return download
                ? controller.PhysicalFile(path, FileStore.ContentType(name), name)
                : controller.PhysicalFile(path, FileStore.ContentType(name), enableRangeProcessing: true);
        }

        private ActionResult Serve(string? storedName, string? originalName, bool download)
        {
            if (!download)
            {
                string name = string.IsNullOrWhiteSpace(originalName) ? storedName ?? "" : originalName;
                // FileNameStar encodes non-ASCII names (headers must be ASCII)
                Response.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("inline") { FileNameStar = name }.ToString();
            }
            return ServeFile(this, storedName, originalName, download);
        }
    }
}
