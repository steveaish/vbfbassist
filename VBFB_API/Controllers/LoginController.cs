using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using VBFB_API.Classes;
using VBFB_API.Models;

namespace VBFB_API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST api/v1/login  { email, password }
        [HttpPost]
        public ActionResult<dynamic> Post([FromBody] JObject data)
        {
            string email = (string?)data["email"] ?? "";
            string password = (string?)data["password"] ?? "";

            LoginResult check = Login.CheckCredentials(_context, email, password);
            if (!check.Success || check.User == null)
            {
                Logging.Log(Logging.LogLevel.Warning, "Failed login for " + email + ": " + check.Message);
                return Unauthorized(new { message = check.Message });
            }

            users user = _context.users.First(u => u.id == check.User.id);
            user.userkey = Guid.NewGuid().ToString();
            user.userkeyexpiry = DateTime.Now.AddHours(Globals.Ready.UserKeyHours);

            if (check.Profile != null)
            {
                _context.program_log.Add(new program_log
                {
                    userid = check.Profile.id,
                    type = 0,
                    description = "Login - " + check.Profile.firstname + " " + check.Profile.lastname,
                    posttime = DateTime.Now
                });
            }
            _context.SaveChanges();

            return Ok(BuildSession(user, check.Profile));
        }

        // GET api/v1/login/me - refreshes the session details (permissions can change while logged in)
        [HttpGet("me")]
        public ActionResult<dynamic> Me()
        {
            users? user = _context.users.FirstOrDefault(u => u.id == ApiUtils.GetUserId(HttpContext));
            if (user == null) return Unauthorized();
            return Ok(BuildSession(user, Login.GetProfileForEmail(_context, user.email)));
        }

        // POST api/v1/login/logout
        [HttpPost("logout")]
        public ActionResult Logout()
        {
            users? user = _context.users.FirstOrDefault(u => u.id == ApiUtils.GetUserId(HttpContext));
            if (user != null)
            {
                user.userkey = null;
                user.userkeyexpiry = null;
                _context.SaveChanges();
            }
            return Ok();
        }

        // POST api/v1/login/changepassword  { currentpassword, newpassword }
        [HttpPost("changepassword")]
        public ActionResult ChangePassword([FromBody] JObject data)
        {
            users? user = _context.users.FirstOrDefault(u => u.id == ApiUtils.GetUserId(HttpContext));
            if (user == null) return Unauthorized();

            string current = (string?)data["currentpassword"] ?? "";
            string newPassword = (string?)data["newpassword"] ?? "";

            if (!Login.VerifyPassword(current, user.password)) return BadRequest(new { message = "Your current password is incorrect." });
            if (newPassword.Length < 6) return BadRequest(new { message = "The new password must be at least 6 characters." });

            user.password = Login.HashPassword(newPassword);
            user.updated_at = DateTime.Now;
            _context.SaveChanges();
            return Ok();
        }

        // POST api/v1/login/forgotpassword { email } - emails a one-hour reset link (PasswordReset). Always answers the
        // same way so the screen can't be used to find out which emails have logins.
        [HttpPost("forgotpassword")]
        public ActionResult ForgotPassword([FromBody] JObject data)
        {
            const string reply = "If that email has a login, a reset link has been sent to it.";
            string email = ((string?)data["email"])?.Trim() ?? "";
            if (email == "") return BadRequest(new { message = "Please enter your email address." });

            // Some people have two logins differing only in case; reset each active one
            List<users> matches = _context.users.Where(u => u.email == email && u.status == 1).ToList();
            foreach (users user in matches)
            {
                try
                {
                    PasswordReset.SendLink(_context, user, newLogin: false, TimeSpan.FromHours(1));
                }
                catch (Exception ex)
                {
                    Logging.Log(Logging.LogLevel.Error, $"Password reset email to user {user.id} failed: {ex.Message}");
                }
            }
            if (matches.Count == 0) Logging.Log(Logging.LogLevel.Info, "Password reset requested for an email with no active login.");

            return Ok(new { message = reply });
        }

        // POST api/v1/login/resetpassword { id, token, password }
        [HttpPost("resetpassword")]
        public ActionResult ResetPassword([FromBody] JObject data)
        {
            int id = (int?)data["id"] ?? 0;
            string token = (string?)data["token"] ?? "";
            string password = (string?)data["password"] ?? "";

            users? user = _context.users.FirstOrDefault(u => u.id == id);
            if (user == null || !PasswordReset.IsValid(user, token)) return BadRequest(new { message = "This reset link is invalid or has expired. Please ask for a new one." });
            if (password.Length < 6) return BadRequest(new { message = "The new password must be at least 6 characters." });

            user.password = Login.HashPassword(password);
            user.resettokenhash = null;
            user.resettokenexpiry = null;
            user.userkey = null; // sign out any existing sessions
            user.updated_at = DateTime.Now;
            _context.SaveChanges();
            Logging.Log(Logging.LogLevel.Info, $"Password reset for user {user.id}.");
            return Ok(new { message = "Your password has been changed. Please sign in." });
        }

        private static object BuildSession(users user, profiles? profile)
        {
            return new
            {
                userid = user.id,
                userkey = user.userkey,
                name = user.name,
                email = user.email,
                profileid = profile?.id,
                firstname = profile?.firstname ?? user.name,
                permissions = Login.BuildPermissions(user, profile),
            };
        }
    }
}
