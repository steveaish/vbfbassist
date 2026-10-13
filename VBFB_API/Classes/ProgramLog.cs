using VBFB_API.Models;

namespace VBFB_API.Classes
{
    /// <summary>
    /// The program_log audit trail, written the same way as Laravel (userid = the profile id).
    /// Types used by Laravel: 0 login, 1 profiles, 2 members, 3 BFR, 4 bulk SMS, 5 cadets,
    /// 6 vehicles / PPE setup screens, 7 documents, 10 calendar.
    /// </summary>
    public static class ProgramLog
    {
        public const int Login = 0;
        public const int Profiles = 1;
        public const int Members = 2;
        public const int Bfr = 3;
        public const int BulkSms = 4;
        public const int Cadets = 5;
        public const int Vehicles = 6;
        public const int Documents = 7;
        public const int Calendar = 10;

        public static void Write(HttpContext context, ApplicationDbContext db, int type, string description)
        {
            profiles? profile = Permissions.CurrentProfile(context, db);
            if (profile == null) return;

            db.program_log.Add(new program_log
            {
                userid = profile.id,
                type = type,
                description = description,
                posttime = DateTime.Now
            });
            db.SaveChanges();
        }
    }
}
