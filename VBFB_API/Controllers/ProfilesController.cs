using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using VBFB_API.Classes;
using VBFB_API.Models;

namespace VBFB_API.Controllers
{
    /// <summary>Admin maintenance of member profiles (Laravel ProfilesController).</summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [RequirePermission("isadmin")]
    public class ProfilesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProfilesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST api/v1/profiles/list - Syncfusion grid (also used for Excel export, which asks for every row)
        [HttpPost("list")]
        public ActionResult<dynamic> List([FromBody] JObject? data)
        {
            ApiBreakDown grid = ApiUtils.SyncFusionBreakDownApiBody(data, "firstname, lastname");
            string filters = ProfileFields.BuildFilters(data, grid.parameters);

            string from = $"from profiles p left join Crews c on p.crew = c.seqno where 1=1 {filters} {grid.where}";

            var rows = SqlQuery.Select(_context,
                $@"select * from (select {ProfileFields.SafeColumns}, c.CrewName, {ProfileFields.MedalNotesSql} as Notes {from}) x
                   order by {grid.orderby} {grid.skipandfetch}", grid.parameters);
            int count = SqlQuery.Count(_context, "select count(*) " + from, grid.parameters);

            return Ok(new { result = rows, count });
        }

        // POST api/v1/profiles/opened - audit entry when the screen is opened (Laravel logged it on every list load)
        [HttpPost("opened")]
        public ActionResult Opened()
        {
            ProgramLog.Write(HttpContext, _context, ProgramLog.Profiles, "Open Profile Screen");
            return Ok();
        }

        // GET api/v1/profiles/lookups - drop-down lists for the edit screen
        [HttpGet("lookups")]
        public ActionResult<dynamic> Lookups()
        {
            return Ok(new
            {
                crews = _context.Crews.AsNoTracking().Where(c => c.deleted != "Y").OrderBy(c => c.CrewName).Select(c => new { c.seqno, c.CrewName }).ToList(),
                cadetcrews = _context.Cadet_Crews.AsNoTracking().OrderBy(c => c.CrewName).Select(c => new { c.seqno, c.CrewName }).ToList(),
                streets = SqlQuery.Select(_context, @"select s.id, a.AreaName, s.StreetName from BFR_Streets s inner join BFR_Areas a on s.AreaID = a.id order by a.AreaName, s.StreetName"),
                ppestyles = PpeStockController.AvailableStyles(_context)
            });
        }

        // GET api/v1/profiles/5
        [HttpGet("{id:int}")]
        public ActionResult<dynamic> Get(int id)
        {
            profiles? profile = _context.profiles.AsNoTracking().FirstOrDefault(p => p.id == id);
            if (profile == null) return NotFound();

            return Ok(new
            {
                profile = ProfileFields.ToSafeDictionary(profile),
                dob_iso = DateUtils.ToIso(profile.dob, profile.dob_date),
                startdate_iso = DateUtils.ToIso(profile.startdate, profile.startdate_date),
                // BFR streets this member may see (Laravel's query dropped streets assigned to other members)
                allowedstreets = _context.BFR_StreetView.AsNoTracking().Where(s => s.profileid == id).Select(s => s.streetid).ToList(),
                ppe = PpeStockController.ForProfile(_context, id)
            });
        }

        // POST api/v1/profiles - create
        [HttpPost]
        public ActionResult<dynamic> Create([FromBody] JObject data)
        {
            profiles profile = new()
            {
                isactive = "Y",
                created_at = DateTime.Now,
                updated_at = DateTime.Now,
            };
            EntityUpdater.Fill(profile, data, ProfileFields.AdminFillable);
            ProfileFields.ApplyDates(profile, data);

            if (string.IsNullOrWhiteSpace(profile.firstname) && string.IsNullOrWhiteSpace(profile.lastname))
                return BadRequest(new { message = "Please enter a first or last name." });

            _context.profiles.Add(profile);
            _context.SaveChanges();
            SaveAllowedStreets(profile.id, data);
            return Ok(new { profile.id });
        }

        // PUT api/v1/profiles/5
        [HttpPut("{id:int}")]
        public ActionResult Update(int id, [FromBody] JObject data)
        {
            profiles? profile = _context.profiles.FirstOrDefault(p => p.id == id);
            if (profile == null) return NotFound();

            EntityUpdater.Fill(profile, data, ProfileFields.AdminFillable);
            ProfileFields.ApplyDates(profile, data);
            profile.updated_at = DateTime.Now;
            _context.SaveChanges();
            SaveAllowedStreets(id, data);
            return Ok();
        }

        // DELETE api/v1/profiles/5 - Laravel hard-deletes; members are normally made inactive instead
        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            profiles? profile = _context.profiles.FirstOrDefault(p => p.id == id);
            if (profile == null) return NotFound();

            _context.BFR_StreetView.RemoveRange(_context.BFR_StreetView.Where(s => s.profileid == id));
            _context.profiles.Remove(profile);
            _context.SaveChanges();
            return Ok();
        }

        private void SaveAllowedStreets(int profileId, JObject data)
        {
            if (data["allowedstreets"] is not JArray streets) return;

            _context.BFR_StreetView.RemoveRange(_context.BFR_StreetView.Where(s => s.profileid == profileId));
            foreach (int streetId in streets.Select(s => (int)s).Distinct())
            {
                _context.BFR_StreetView.Add(new BFR_StreetView { profileid = profileId, streetid = streetId });
            }
            _context.SaveChanges();
        }
    }
}
