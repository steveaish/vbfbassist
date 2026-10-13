using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using VBFB_API.Classes;
using VBFB_API.Models;

namespace VBFB_API.Controllers
{
    /// <summary>
    /// Vehicle checks (Laravel VehiclesController index/create/update/signoff/reports).
    /// Status: 0 New, 1 In Progress, 2 Check Complete (fails without a remedy), 3 Ready To Sign Off, 4 Signed Off.
    /// Line results: 'Not Checked', 'Checked Pass', 'Checked Fail'.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [RequirePermission("isvehiclemaint")]
    public class VehicleChecksController : ControllerBase
    {
        public const int SignedOff = 4;
        private static readonly string[] Results = { "Not Checked", "Checked Pass", "Checked Fail" };

        private readonly ApplicationDbContext _context;

        public VehicleChecksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST api/v1/vehiclechecks/opened
        [HttpPost("opened")]
        public ActionResult Opened()
        {
            ProgramLog.Write(HttpContext, _context, ProgramLog.Vehicles, "Open Vehicle Checks Screen");
            return Ok();
        }

        // GET api/v1/vehiclechecks?showsignedoff=Y
        [HttpGet]
        public ActionResult<dynamic> List([FromQuery] string? showsignedoff)
        {
            string signedOff = showsignedoff == "Y" ? "" : $"and c.status < {SignedOff}";
            return Ok(SqlQuery.Select(_context, $@"select c.id, c.vehicleid, c.createddate, c.monthly, c.status, c.signedoffname, v.Name,
                    (p.firstname + ' ' + p.lastname) as CreatedByName
                from VEH_Check_Header c
                inner join VEH_Vehicles v on c.vehicleid = v.id
                left join profiles p on c.createdby = p.id
                where c.deleted = 'N' {signedOff}
                order by c.createddate desc"));
        }

        // GET api/v1/vehiclechecks/newcheck - choices for starting a check
        [HttpGet("newcheck")]
        public ActionResult<dynamic> NewCheck()
        {
            return Ok(new
            {
                // A vehicle with a check in progress can't start another
                vehicles = SqlQuery.Select(_context, $@"select id, Name, NumberPlate from VEH_Vehicles
                    where deleted = 'N' and id not in (select vehicleid from VEH_Check_Header where status < {SignedOff} and deleted = 'N')
                    order by Name"),
                checkers = _context.profiles.AsNoTracking().Where(p => p.IsVehicleMaint == "Y" && p.isactive == "Y")
                    .OrderBy(p => p.firstname).ThenBy(p => p.lastname).Select(p => new { p.id, name = p.firstname + " " + p.lastname }).ToList(),
                myprofileid = Permissions.CurrentProfile(HttpContext, _context)?.id
            });
        }

        // POST api/v1/vehiclechecks { vehicleid, createdby, monthly: Y|N }
        [HttpPost]
        public ActionResult<dynamic> Create([FromBody] JObject data)
        {
            int vehicleId = (int?)data["vehicleid"] ?? 0;
            string monthly = (string?)data["monthly"] == "N" ? "N" : "Y";

            if (!_context.VEH_Vehicles.Any(v => v.id == vehicleId && v.deleted == "N")) return BadRequest(new { message = "Please choose a vehicle." });
            if (_context.VEH_Check_Header.Any(h => h.vehicleid == vehicleId && h.status < SignedOff && h.deleted == "N"))
                return BadRequest(new { message = "This vehicle already has a check in progress. Please complete it first." });

            VEH_Check_Header header = new()
            {
                vehicleid = vehicleId,
                createdby = (int?)data["createdby"] ?? Permissions.CurrentProfile(HttpContext, _context)?.id,
                createddate = DateTime.Now,
                monthly = monthly,
                status = 0,
                deleted = "N"
            };
            _context.VEH_Check_Header.Add(header);
            _context.SaveChanges();

            // Copy the vehicle's checklist (weekly or monthly items) in section then item order.
            // The column name is one of two constants; the values are parameters.
            string frequencyColumn = monthly == "Y" ? "monthly" : "weekly";
            string copySql = $@"insert into VEH_Check_Lines (checkid, sectionid, itemid)
                select @checkid, i.sectionid, i.itemid
                from VEH_Sec_Itm i
                inner join VEH_Veh_Sec s on i.sectionid = s.sectionid and i.vehicleid = s.vehicleid
                where i.vehicleid = @vehicleid and i.deleted = 'N' and s.deleted = 'N' and i.{frequencyColumn} = 'Y'
                order by s.orderby, i.orderby";
            _context.Database.ExecuteSqlRaw(copySql, new SqlParameter("@checkid", header.id), new SqlParameter("@vehicleid", vehicleId));

            return Ok(new { header.id });
        }

        // GET api/v1/vehiclechecks/5
        [HttpGet("{id:int}")]
        public ActionResult<dynamic> Get(int id)
        {
            VehicleReports.CheckData? data = VehicleReports.Load(_context, id);
            if (data == null || data.Header["deleted"]?.ToString() == "Y") return NotFound();

            return Ok(new
            {
                check = data.Header,
                lines = data.Lines,
                resolutions = _context.VEH_Resolutions.AsNoTracking().OrderBy(r => r.id).Select(r => r.Resolution).ToList()
            });
        }

        // PUT api/v1/vehiclechecks/5/lines  [{ id, checked?, comments?, remedy? }, ...]
        // Only the fields sent are changed (Laravel blanked every remedy when saving outside "complete" mode).
        [HttpPut("{id:int}/lines")]
        public ActionResult<dynamic> UpdateLines(int id, [FromBody] JArray lines)
        {
            VEH_Check_Header? header = _context.VEH_Check_Header.FirstOrDefault(h => h.id == id && h.deleted == "N");
            if (header == null) return NotFound();
            if (header.status >= SignedOff) return BadRequest(new { message = "This check has been signed off and can't be changed." });

            foreach (JObject line in lines.OfType<JObject>())
            {
                int lineId = (int?)line["id"] ?? 0;
                List<string> sets = new();
                List<SqlParameter> ps = new() { new SqlParameter("@id", lineId), new SqlParameter("@checkid", id) };

                if (line.ContainsKey("checked"))
                {
                    string result = (string?)line["checked"] ?? "";
                    if (!Results.Contains(result)) return BadRequest(new { message = "Unknown result " + result });
                    sets.Add("checked = @checked");
                    ps.Add(new SqlParameter("@checked", result));
                }
                if (line.ContainsKey("comments"))
                {
                    sets.Add("comments = @comments");
                    ps.Add(new SqlParameter("@comments", (object?)(string?)line["comments"] ?? DBNull.Value));
                }
                if (line.ContainsKey("remedy"))
                {
                    sets.Add("remedy = @remedy");
                    ps.Add(new SqlParameter("@remedy", (object?)(string?)line["remedy"] ?? DBNull.Value));
                }
                if (sets.Count == 0) continue;

                // sets holds fixed "column = @param" fragments only
                string updateSql = $"update VEH_Check_Lines set {string.Join(", ", sets)} where id = @id and checkid = @checkid";
                _context.Database.ExecuteSqlRaw(updateSql, ps);
            }

            header.status = CalculateStatus(id);
            _context.SaveChanges();
            return Ok(new { header.status });
        }

        // Same rules as Laravel's update()
        private int CalculateStatus(int checkId)
        {
            var counts = SqlQuery.Select(_context, @"select count(*) as total,
                    sum(case when checked <> 'Not Checked' then 1 else 0 end) as done,
                    sum(case when checked = 'Checked Fail' and isnull(remedy, '') = '' then 1 else 0 end) as failnoremedy
                from VEH_Check_Lines where checkid = @id", new[] { new SqlParameter("@id", checkId) })[0];

            int total = Convert.ToInt32(counts["total"]);
            int done = Convert.ToInt32(counts["done"] ?? 0);
            int failNoRemedy = Convert.ToInt32(counts["failnoremedy"] ?? 0);

            if (done == 0) return 0;
            if (done < total) return 1;
            return failNoRemedy == 0 ? 3 : 2;
        }

        // DELETE api/v1/vehiclechecks/5
        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            VEH_Check_Header? header = _context.VEH_Check_Header.FirstOrDefault(h => h.id == id && h.deleted == "N");
            if (header == null) return NotFound();
            if (header.status >= SignedOff) return BadRequest(new { message = "A signed off check can't be deleted." });

            header.deleted = "Y";
            header.status = SignedOff;
            _context.SaveChanges();
            return Ok();
        }

        // POST api/v1/vehiclechecks/5/signoff { signoffcode } - any profile's sign-off code, then the report is emailed
        [HttpPost("{id:int}/signoff")]
        public ActionResult<dynamic> SignOff(int id, [FromBody] JObject data)
        {
            VEH_Check_Header? header = _context.VEH_Check_Header.FirstOrDefault(h => h.id == id && h.deleted == "N");
            if (header == null) return NotFound();
            if (header.status != 3) return BadRequest(new { message = "Every item must be checked, and every fail given a remedy, before sign off." });

            string code = ((string?)data["signoffcode"])?.Trim() ?? "";
            profiles? signer = code == "" ? null : _context.profiles.AsNoTracking().Where(p => p.signoffcode == code).OrderBy(p => p.id).FirstOrDefault();
            if (signer == null)
            {
                Logging.Log(Logging.LogLevel.Warning, $"Incorrect vehicle sign off code for check {id}");
                return BadRequest(new { message = "Incorrect code." });
            }

            header.status = SignedOff;
            header.signedoffid = signer.id;
            header.signedoffname = $"{signer.firstname} {signer.lastname}";
            header.signeddate = DateTime.Now;
            header.signedby = signer.id;
            _context.SaveChanges();

            // Signing off stands even if the email fails; the user can resend it
            string? emailError = TryEmailReport(id);
            return Ok(new { emailed = emailError == null, emailError });
        }

        // POST api/v1/vehiclechecks/5/emailreport - to the logged-in user, as Laravel did
        [HttpPost("{id:int}/emailreport")]
        public ActionResult EmailReport(int id)
        {
            string? error = TryEmailReport(id);
            if (error != null) return BadRequest(new { message = error });
            return Ok(new { message = "The report has been emailed to " + CurrentEmail() });
        }

        // GET api/v1/vehiclechecks/5/report?apikey=&userid=&userkey= - opened in a new tab
        [HttpGet("{id:int}/report")]
        public ActionResult Report(int id)
        {
            VehicleReports.CheckData? data = VehicleReports.Load(_context, id);
            if (data == null) return NotFound();
            Response.Headers.ContentDisposition = $"inline; filename=\"{data.FileStem}.pdf\"";
            return File(VehicleReports.InspectionPdf(data), "application/pdf");
        }

        // GET api/v1/vehiclechecks/5/defectreport
        [HttpGet("{id:int}/defectreport")]
        public ActionResult DefectReport(int id)
        {
            VehicleReports.CheckData? data = VehicleReports.Load(_context, id);
            if (data == null) return NotFound();
            Response.Headers.ContentDisposition = $"inline; filename=\"Defect Report {data.FileStem}.pdf\"";
            return File(VehicleReports.DefectPdf(data), "application/pdf");
        }

        private string? CurrentEmail() => (HttpContext.Items["User"] as users)?.email;

        private string? TryEmailReport(int id)
        {
            VehicleReports.CheckData? data = VehicleReports.Load(_context, id);
            if (data == null) return "Check not found.";
            string? email = CurrentEmail();
            if (string.IsNullOrEmpty(email)) return "Your login has no email address.";

            try
            {
                Mailer.Send(new[] { email },
                    $"Vehicle Inspection Report {data.VehicleName} {data.CreatedDate:dd/MM/yyyy}",
                    "Please find attached your vehicle inspection report.",
                    new[]
                    {
                        new Mailer.Attachment { FileName = data.FileStem + ".pdf", Content = VehicleReports.InspectionPdf(data), ContentType = "application/pdf" },
                        new Mailer.Attachment { FileName = "VehicleInspectionReport.csv", Content = VehicleReports.InspectionCsv(data), ContentType = "text/csv" }
                    });
                return null;
            }
            catch (Exception ex)
            {
                Logging.Log(Logging.LogLevel.Error, $"Emailing vehicle report {id} failed: {ex.Message}");
                return "The report could not be emailed: " + ex.Message;
            }
        }
    }
}
