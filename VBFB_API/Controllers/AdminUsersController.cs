using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using VBFB_API.Classes;
using VBFB_API.Models;

namespace VBFB_API.Controllers
{
    /// <summary>
    /// Admin > Users (Laravel AdminController users/usersEdit): the logins in users. Permissions come
    /// from the member profile with the same email, so this screen shows that link and can create a
    /// login straight from a profile. Logins are deactivated (status 0), never deleted. Site admins only.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [RequirePermission("siteadmin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminUsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/v1/adminusers - logins with their matching profile and last login
        [HttpGet]
        public ActionResult<dynamic> List()
        {
            return Ok(SqlQuery.Select(_context, @"select u.id, u.name, u.email, u.status, u.admin, u.created_at, u.updated_at,
                    p.id as profileid, (p.firstname + ' ' + p.lastname) as ProfileName, p.isactive as ProfileActive, p.alltypes,
                    (select max(l.posttime) from program_log l where l.type = 0 and l.userid = p.id) as LastLogin
                from users u
                outer apply (select top 1 * from profiles pr where pr.email = u.email order by case when pr.isactive = 'Y' then 0 else 1 end, pr.id) p
                order by u.name"));
        }

        // GET api/v1/adminusers/candidates - active profiles with an email but no login yet
        [HttpGet("candidates")]
        public ActionResult<dynamic> Candidates()
        {
            return Ok(SqlQuery.Select(_context, @"select p.id, p.firstname, p.lastname, p.email, p.alltypes
                from profiles p
                where p.isactive = 'Y' and isnull(p.email, '') <> '' and not exists (select 1 from users u where u.email = p.email)
                order by p.firstname, p.lastname"));
        }

        // POST api/v1/adminusers { name, email, password?, admin, sendlink }
        // Without a password, sendlink emails the person a link to choose one (valid 7 days).
        [HttpPost]
        public ActionResult<dynamic> Create([FromBody] JObject data)
        {
            string name = ((string?)data["name"])?.Trim() ?? "";
            string email = ((string?)data["email"])?.Trim() ?? "";
            string password = (string?)data["password"] ?? "";
            bool sendLink = (bool?)data["sendlink"] == true;

            if (name == "" || email == "") return BadRequest(new { message = "Please enter a name and email." });
            if (_context.users.Any(u => u.email == email)) return BadRequest(new { message = "There is already a login with that email." });
            if (password == "" && !sendLink) return BadRequest(new { message = "Enter a password or choose to email a set-password link." });
            if (password != "" && password.Length < 6) return BadRequest(new { message = "Passwords must be at least 6 characters." });

            users user = new()
            {
                company = 1,
                name = name,
                email = email,
                // No usable password until they follow the link
                password = Login.HashPassword(password != "" ? password : Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(24))),
                status = 1,
                admin = (bool?)data["admin"] == true ? 1 : 0,
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };
            _context.users.Add(user);
            _context.SaveChanges();

            string? emailError = null;
            if (sendLink)
            {
                try { PasswordReset.SendLink(_context, user, newLogin: true, TimeSpan.FromDays(7)); }
                catch (Exception ex) { emailError = "The login was created but the email could not be sent: " + ex.Message; }
            }
            return Ok(new { user.id, emailError });
        }

        // PUT api/v1/adminusers/5 { name, email, status, admin, password? }
        [HttpPut("{id:int}")]
        public ActionResult Update(int id, [FromBody] JObject data)
        {
            users? user = _context.users.FirstOrDefault(u => u.id == id);
            if (user == null) return NotFound();

            string name = ((string?)data["name"])?.Trim() ?? "";
            string email = ((string?)data["email"])?.Trim() ?? "";
            if (name == "" || email == "") return BadRequest(new { message = "Please enter a name and email." });
            if (_context.users.Any(u => u.email == email && u.id != id)) return BadRequest(new { message = "Another login already uses that email." });

            int status = (bool?)data["status"] == false ? 0 : 1;
            int admin = (bool?)data["admin"] == true ? 1 : 0;
            if (id == ApiUtils.GetUserId(HttpContext) && (status == 0 || admin == 0))
                return BadRequest(new { message = "You can't deactivate your own login or remove your own admin access." });

            string password = (string?)data["password"] ?? "";
            if (password != "" && password.Length < 6) return BadRequest(new { message = "Passwords must be at least 6 characters." });

            user.name = name;
            user.email = email;
            user.status = status;
            user.admin = admin;
            if (password != "") user.password = Login.HashPassword(password);
            if (status == 0 || password != "") user.userkey = null; // sign them out
            user.updated_at = DateTime.Now;
            _context.SaveChanges();
            return Ok();
        }

        // POST api/v1/adminusers/5/sendlink - email a set-password link (valid 7 days)
        [HttpPost("{id:int}/sendlink")]
        public ActionResult SendLink(int id)
        {
            users? user = _context.users.FirstOrDefault(u => u.id == id);
            if (user == null) return NotFound();
            if (user.status != 1) return BadRequest(new { message = "Activate the login first." });
            try
            {
                PasswordReset.SendLink(_context, user, newLogin: false, TimeSpan.FromDays(7));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "The email could not be sent: " + ex.Message });
            }
            return Ok(new { message = $"A set-password link has been emailed to {user.email}." });
        }
    }
}
