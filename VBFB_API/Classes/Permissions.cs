using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using VBFB_API.Models;

namespace VBFB_API.Classes
{
    /// <summary>
    /// The logged-in user's permission flags (see Login.BuildPermissions), worked out once per request
    /// and cached in HttpContext.Items.
    /// </summary>
    public static class Permissions
    {
        public static Dictionary<string, object?> Get(HttpContext context, ApplicationDbContext db)
        {
            if (context.Items["Permissions"] is Dictionary<string, object?> cached) return cached;

            Dictionary<string, object?> result = new();
            if (context.Items["User"] is users user)
            {
                profiles? profile = Login.GetProfileForEmail(db, user.email);
                context.Items["Profile"] = profile;
                result = Login.BuildPermissions(user, profile);
            }
            context.Items["Permissions"] = result;
            return result;
        }

        public static bool Has(HttpContext context, ApplicationDbContext db, string permission) =>
            Get(context, db).TryGetValue(permission, out object? v) && v is true;

        /// <summary>The profiles row of the logged-in user (null when their email has no profile).</summary>
        public static profiles? CurrentProfile(HttpContext context, ApplicationDbContext db)
        {
            Get(context, db);
            return context.Items["Profile"] as profiles;
        }
    }

    /// <summary>
    /// Returns 403 unless the user has at least one of the listed permission flags.
    /// [RequirePermission("isadmin")] or [RequirePermission("cadetleader", "isadmin")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionAttribute : ActionFilterAttribute
    {
        private readonly string[] _permissions;

        public RequirePermissionAttribute(params string[] permissions)
        {
            _permissions = permissions;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ApplicationDbContext db = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            if (!_permissions.Any(p => Permissions.Has(context.HttpContext, db, p)))
            {
                context.Result = new ObjectResult(new { message = "You don't have access to this." }) { StatusCode = StatusCodes.Status403Forbidden };
            }
        }
    }
}
