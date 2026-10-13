using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using VBFB_API.Classes;
using VBFB_API.Models;

namespace VBFB_API.Controllers
{
    /// <summary>The logged-in member's own details and PPE (Laravel MembersController@mypage).</summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    public class MyPageController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MyPageController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/v1/mypage
        [HttpGet]
        public ActionResult<dynamic> Get()
        {
            profiles? me = Permissions.CurrentProfile(HttpContext, _context);
            if (me == null) return Ok(new { profile = (object?)null, ppe = Array.Empty<object>() });

            var rows = SqlQuery.Select(_context,
                $"select {ProfileFields.MemberViewColumns} from profiles p left join Crews c on p.crew = c.seqno where p.id = @id",
                new[] { new SqlParameter("@id", me.id) });

            return Ok(new
            {
                profile = rows.FirstOrDefault(),
                ppe = PpeStockController.ForProfile(_context, me.id)
            });
        }
    }
}
