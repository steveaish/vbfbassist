using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Globalization;
using VBFB_API.Classes;
using VBFB_API.Models;

namespace VBFB_API.Controllers
{
    /// <summary>
    /// Bush Fire Ready (Laravel BfrController): streets by area with activity counts, and the
    /// community activities logged against each street. Facilitators see every street; street
    /// coordinators see only the streets assigned to them on their profile (BFR_StreetView).
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [RequirePermission("isfacilitator", "iscoordinator")]
    public class BfrController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BfrController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool SeesAll => Permissions.Has(HttpContext, _context, "isfacilitator");
        private int MyProfileId => Permissions.CurrentProfile(HttpContext, _context)?.id ?? 0;

        // Street filter for coordinators
        private string StreetScope(List<SqlParameter> ps, string streetColumn)
        {
            if (SeesAll) return "";
            ps.Add(new SqlParameter("@me", MyProfileId));
            return $"and {streetColumn} in (select streetid from BFR_StreetView where profileid = @me)";
        }

        private bool CanSeeStreet(int streetId) =>
            SeesAll || _context.BFR_StreetView.Any(v => v.profileid == MyProfileId && v.streetid == streetId);

        [HttpPost("opened")]
        public ActionResult Opened()
        {
            ProgramLog.Write(HttpContext, _context, ProgramLog.Bfr, "Open BFR Screen");
            return Ok();
        }

        // GET api/v1/bfr/streets?search=&area=
        [HttpGet("streets")]
        public ActionResult<dynamic> Streets([FromQuery] string? search, [FromQuery] string? area)
        {
            List<SqlParameter> ps = new();
            string where = StreetScope(ps, "s.id");
            if (!string.IsNullOrWhiteSpace(search))
            {
                ps.Add(new SqlParameter("@search", search.Trim()));
                where += " and (a.AreaName like '%' + @search + '%' or s.StreetName like '%' + @search + '%')";
            }
            if (!string.IsNullOrWhiteSpace(area) && area != "ALL")
            {
                ps.Add(new SqlParameter("@area", area));
                where += " and a.AreaName = @area";
            }

            // Laravel's three-month count compared the wrong way round and counted every past activity
            var streets = SqlQuery.Select(_context, $@"select s.id, a.AreaName, s.StreetName,
                    (select count(*) from BFR_Activities where streetid = s.id and activitydate >= datefromparts(year(getdate()), month(getdate()), 1)) as ThisMonth,
                    (select count(*) from BFR_Activities where streetid = s.id and activitydate >= dateadd(month, -3, cast(getdate() as date))) as ThreeMonths,
                    (select count(*) from BFR_Activities where streetid = s.id and activitydate >= datefromparts(year(getdate()), 1, 1)) as ThisYear
                from BFR_Streets s inner join BFR_Areas a on s.AreaID = a.id
                where 1=1 {where}
                order by a.AreaName, s.StreetName", ps);

            List<SqlParameter> aps = new();
            var areas = SqlQuery.Select(_context, $@"select distinct a.AreaName
                from BFR_Streets s inner join BFR_Areas a on s.AreaID = a.id
                where 1=1 {StreetScope(aps, "s.id")}
                order by a.AreaName", aps).Select(r => r["AreaName"]).ToList();

            return Ok(new { streets, areas, seesall = SeesAll });
        }

        // GET api/v1/bfr/streets/12 - activities and the residents' phone list
        [HttpGet("streets/{id:int}")]
        public ActionResult<dynamic> Street(int id)
        {
            if (!CanSeeStreet(id)) return StatusCode(StatusCodes.Status403Forbidden);
            SqlParameter[] p = { new("@id", id) };

            var street = SqlQuery.Select(_context, @"select s.id, a.AreaName, s.StreetName
                from BFR_Streets s inner join BFR_Areas a on s.AreaID = a.id where s.id = @id", p);
            if (street.Count == 0) return NotFound();

            return Ok(new
            {
                street = street[0],
                activities = SqlQuery.Select(_context, @"select act.id, act.activityid, act.profileid, act.streetid, act.activitydate, act.starttime,
                        act.duration, act.notes, act.attendeed, t.ActivityDesc, (p.firstname + ' ' + p.lastname) as MemberName
                    from BFR_Activities act
                    left join BFR_ActivityTypes t on act.activityid = t.id
                    left join profiles p on act.profileid = p.id
                    where act.streetid = @id
                    order by act.activitydate desc, act.id desc", p),
                phonelist = SqlQuery.Select(_context, @"select id, firstname, lastname, mobilephone, homephone
                    from profiles where streetid = @id and isactive = 'Y' order by firstname, lastname", p)
            });
        }

        // GET api/v1/bfr/lookups - choices for the activity form
        [HttpGet("lookups")]
        public ActionResult<dynamic> Lookups()
        {
            List<SqlParameter> ps = new();
            return Ok(new
            {
                streets = SqlQuery.Select(_context, $@"select s.id, a.AreaName, s.StreetName
                    from BFR_Streets s inner join BFR_Areas a on s.AreaID = a.id
                    where 1=1 {StreetScope(ps, "s.id")} order by a.AreaName, s.StreetName", ps),
                activitytypes = _context.BFR_ActivityTypes.AsNoTracking().Where(t => t.id > 0).OrderBy(t => t.ActivityDesc).Select(t => new { t.id, t.ActivityDesc }).ToList(),
                members = _context.profiles.AsNoTracking().Where(p => (p.isfacilitator == "Y" || p.iscoordinator == "Y") && p.isactive == "Y")
                    .OrderBy(p => p.firstname).ThenBy(p => p.lastname).Select(p => new { p.id, name = p.firstname + " " + p.lastname }).ToList(),
                myprofileid = MyProfileId
            });
        }

        // POST api/v1/bfr/activities { streetid, activityid, profileid, activitydate (yyyy-MM-dd), starttime, duration, attendeed, notes }
        [HttpPost("activities")]
        public ActionResult<dynamic> Create([FromBody] JObject data)
        {
            BFR_Activities act = new() { enteredby = MyProfileId };
            string? error = Apply(act, data);
            if (error != null) return BadRequest(new { message = error });
            _context.BFR_Activities.Add(act);
            _context.SaveChanges();
            return Ok(new { act.id });
        }

        [HttpPut("activities/{id:int}")]
        public ActionResult Update(int id, [FromBody] JObject data)
        {
            BFR_Activities? act = _context.BFR_Activities.FirstOrDefault(a => a.id == id);
            if (act == null || !CanSeeStreet(act.streetid ?? 0)) return NotFound();
            string? error = Apply(act, data);
            if (error != null) return BadRequest(new { message = error });
            _context.SaveChanges();
            return Ok();
        }

        // DELETE api/v1/bfr/activities/5 - facilitators, or whoever entered it
        [HttpDelete("activities/{id:int}")]
        public ActionResult Delete(int id)
        {
            BFR_Activities? act = _context.BFR_Activities.FirstOrDefault(a => a.id == id);
            if (act == null) return NotFound();
            if (!SeesAll && act.enteredby != MyProfileId) return StatusCode(StatusCodes.Status403Forbidden);
            _context.BFR_Activities.Remove(act);
            _context.SaveChanges();
            return Ok();
        }

        private string? Apply(BFR_Activities act, JObject data)
        {
            int streetId = (int?)data["streetid"] ?? 0;
            if (!_context.BFR_Streets.Any(s => s.id == streetId)) return "Please choose a street.";
            if (!CanSeeStreet(streetId)) return "That street is not assigned to you.";
            if (!DateTime.TryParseExact((string?)data["activitydate"], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                return "Please choose a date.";

            act.streetid = streetId;
            act.activitydate = date;
            act.activityid = (int?)data["activityid"] ?? -1;
            act.profileid = (int?)data["profileid"] ?? MyProfileId;
            act.starttime = (string?)data["starttime"];
            act.duration = (double?)data["duration"] ?? 0;
            act.attendeed = (int?)data["attendeed"];
            act.notes = (string?)data["notes"];
            return null;
        }
    }
}
