using Microsoft.EntityFrameworkCore;
using VBFB_API.Classes;
using VBFB_API.Models;

namespace VBFB_API.Middleware
{
    /// <summary>
    /// ReadyApi-style header auth. Every request needs the APIKEY header. Apart from the anonymous paths
    /// below, it also needs UserId + UserKey matching a users row whose key has not expired (the key is
    /// issued by POST api/v1/login). Unlike ReadyApi, a bad UserKey is rejected rather than let through.
    /// </summary>
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        private static readonly string[] AnonymousPaths =
        {
            "/api/v1/login",
            "/api/v1/about",
            "/api/v1/login/forgotpassword",
            "/api/v1/login/resetpassword",
        };

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, ApplicationDbContext db)
        {
            string path = context.Request.Path.ToString().ToLower();

            if (HttpMethods.IsOptions(context.Request.Method) || !path.StartsWith("/api/"))
            {
                await _next.Invoke(context);
                return;
            }

            // File links opened in a new tab can't send headers, so they may pass them as query values
            string? apiKey = Header(context, "APIKEY");
            if (apiKey != Globals.Ready.APIKEY)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            if (AnonymousPaths.Contains(path.TrimEnd('/')))
            {
                await _next.Invoke(context);
                return;
            }

            string? userId = Header(context, "UserId");
            string? userKey = Header(context, "UserKey");

            if (int.TryParse(userId, out int id) && !string.IsNullOrEmpty(userKey))
            {
                users? user = await db.users.AsNoTracking().FirstOrDefaultAsync(u => u.id == id && u.userkey == userKey);
                if (user != null && user.status == 1 && user.userkeyexpiry > DateTime.Now)
                {
                    context.Items["UserId"] = user.id;
                    context.Items["User"] = user;
                    await _next.Invoke(context);
                    return;
                }
            }

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }

        private static string? Header(HttpContext context, string name)
        {
            string? value = context.Request.Headers[name];
            if (string.IsNullOrEmpty(value)) value = context.Request.Query[name.ToLower()];
            return value;
        }
    }
}
