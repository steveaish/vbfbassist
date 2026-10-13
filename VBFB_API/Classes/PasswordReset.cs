using System.Security.Cryptography;
using VBFB_API.Models;

namespace VBFB_API.Classes
{
    /// <summary>
    /// Password reset links: a random token is emailed, only its SHA-256 hash is stored on the user
    /// (users.resettokenhash / resettokenexpiry). Used by "Forgot my password" and by Admin > Users
    /// when an administrator creates a login or sends a set-password link.
    /// </summary>
    public static class PasswordReset
    {
        /// <summary>Creates a token and emails the link. Throws if the email can't be sent.</summary>
        public static void SendLink(ApplicationDbContext db, users user, bool newLogin, TimeSpan validFor)
        {
            if (string.IsNullOrWhiteSpace(user.email)) throw new InvalidOperationException("This login has no email address.");

            string token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
            user.resettokenhash = Hash(token);
            user.resettokenexpiry = DateTime.Now.Add(validFor);
            db.SaveChanges();

            string link = $"{Globals.Ready.AppUrl.TrimEnd('/')}/resetpassword?id={user.id}&token={token}";
            string hours = validFor.TotalHours >= 24 ? $"{(int)validFor.TotalDays} days" : $"{(int)validFor.TotalHours} hour(s)";
            string body = newLogin
                ? $"Hi {user.name},\n\nA VBFB Assist login has been set up for you ({user.email}).\n\nUse this link within {hours} to choose your password:\n\n{link}\n"
                : $"Hi {user.name},\n\nUse this link within {hours} to choose a new password for VBFB Assist:\n\n{link}\n\n" +
                  "If you didn't ask for this you can ignore this email - your password has not changed.";

            Mailer.Send(new[] { user.email }, newLogin ? "Your VBFB Assist login" : "VBFB Assist password reset", body);
        }

        public static bool IsValid(users user, string token)
        {
            if (user.status != 1 || string.IsNullOrEmpty(token) || user.resettokenhash == null || !(user.resettokenexpiry > DateTime.Now)) return false;
            try
            {
                return CryptographicOperations.FixedTimeEquals(Convert.FromHexString(user.resettokenhash), Convert.FromHexString(Hash(token)));
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static string Hash(string token) =>
            Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token)));
    }
}
