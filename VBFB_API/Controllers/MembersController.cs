using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using VBFB_API.Classes;
using VBFB_API.Models;

namespace VBFB_API.Controllers
{
    /// <summary>
    /// Read-only member directory (Laravel MembersController) for people with View Members.
    /// Only contact details and roles are returned - no address, DOB or medical details.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [RequirePermission("viewmembers")]
    public class MembersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MembersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST api/v1/members/opened
        [HttpPost("opened")]
        public ActionResult Opened()
        {
            ProgramLog.Write(HttpContext, _context, ProgramLog.Members, "Open Members Screen");
            return Ok();
        }

        // POST api/v1/members/list - Syncfusion grid; params: search, showinactive
        [HttpPost("list")]
        public ActionResult<dynamic> List([FromBody] JObject? data)
        {
            ApiBreakDown grid = ApiUtils.SyncFusionBreakDownApiBody(data, "firstname, lastname");
            string filters = ProfileFields.BuildFilters(data, grid.parameters);
            string from = $"from profiles p left join Crews c on p.crew = c.seqno where 1=1 {filters} {grid.where}";

            var rows = SqlQuery.Select(_context,
                $@"select * from (select p.id, firstname, lastname, (isnull(firstname, '') + ' ' + isnull(lastname, '')) as fullname,
                        mobilephone, homephone, email, alltypes, isactive, c.CrewName {from}) x
                   order by {grid.orderby} {grid.skipandfetch}", grid.parameters);
            int count = SqlQuery.Count(_context, "select count(*) " + from, grid.parameters);

            return Ok(new { result = rows, count });
        }

        // GET api/v1/members/5
        [HttpGet("{id:int}")]
        public ActionResult<dynamic> Get(int id)
        {
            var rows = SqlQuery.Select(_context,
                $"select {ProfileFields.MemberViewColumns} from profiles p left join Crews c on p.crew = c.seqno where p.id = @id",
                new[] { new SqlParameter("@id", id) });
            if (rows.Count == 0) return NotFound();
            return Ok(rows[0]);
        }
    }
}
