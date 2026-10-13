using VBFB_API.Models;

namespace VBFB_API.Classes
{
    public class LoginResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public users? User { get; set; }
        public profiles? Profile { get; set; }
    }

    /// <summary>
    /// Laravel-compatible login: users.email + bcrypt password, users.status must be 1, and the
    /// permission flags come from the profiles row with the same email.
    /// </summary>
    public static class Login
    {
        public static LoginResult CheckCredentials(ApplicationDbContext context, string email, string password)
        {
            LoginResult result = new();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
            {
                result.Message = "Please enter your email and password.";
                return result;
            }

            // Some people have two users rows whose email differs only by case; accept whichever
            // row the password matches (the database collation is case-insensitive).
            List<users> candidates = context.users.Where(u => u.email == email.Trim()).OrderBy(u => u.id).ToList();
            users? user = candidates.FirstOrDefault(u => VerifyPassword(password, u.password));

            if (user == null)
            {
                result.Message = "These credentials do not match our records.";
                return result;
            }

            if (user.status != 1)
            {
                result.Message = "Your login has not been activated.";
                return result;
            }

            result.Success = true;
            result.User = user;
            result.Profile = GetProfileForEmail(context, user.email);
            return result;
        }

        public static bool VerifyPassword(string password, string? hash)
        {
            if (string.IsNullOrEmpty(hash)) return false;
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                return false;
            }
        }

        // Laravel stores $2y$ hashes with cost 10; keep the same so the Laravel app can still verify
        // passwords set here while both apps are live.
        public static string HashPassword(string password) =>
            BCrypt.Net.BCrypt.HashPassword(password, 10).Replace("$2a$", "$2y$");

        /// <summary>
        /// Several profiles share an email (family members). Prefer the active one, then the lowest id,
        /// which matches what Laravel's Profiles::where('email')->first() normally returned.
        /// </summary>
        public static profiles? GetProfileForEmail(ApplicationDbContext context, string? email)
        {
            if (string.IsNullOrEmpty(email)) return null;
            return context.profiles
                .Where(p => p.email == email)
                .OrderBy(p => p.isactive == "Y" ? 0 : 1)
                .ThenBy(p => p.id)
                .FirstOrDefault();
        }

        public static Dictionary<string, object?> BuildPermissions(users user, profiles? profile)
        {
            static bool Y(string? v) => string.Equals(v?.Trim(), "Y", StringComparison.OrdinalIgnoreCase);
            bool isAdmin = Y(profile?.isadmin);

            return new Dictionary<string, object?>
            {
                ["siteadmin"] = user.admin == 1,
                ["isadmin"] = isAdmin,
                ["cadetleader"] = isAdmin || Y(profile?.cadetleader),
                ["isfacilitator"] = isAdmin || Y(profile?.isfacilitator),
                ["iscoordinator"] = Y(profile?.iscoordinator) || Y(profile?.isstreetcoordinator),
                ["isvehiclemaint"] = isAdmin || Y(profile?.IsVehicleMaint),
                ["viewmembers"] = isAdmin || Y(profile?.ViewMembers),
                ["canbulksms"] = isAdmin || Y(profile?.canbulksms),
                ["caneditdocuments"] = isAdmin || Y(profile?.caneditdocuments),
                ["canseecalendar"] = isAdmin || Y(profile?.canseecalendar),
                ["caneditcalendar"] = isAdmin || Y(profile?.caneditcalendar),
                ["profile_profiles"] = Y(user.profile_profiles),
                ["profile_cadets"] = Y(user.profile_cadets),
            };
        }
    }
}
