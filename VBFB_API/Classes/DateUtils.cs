using System.Globalization;

namespace VBFB_API.Classes
{
    /// <summary>
    /// profiles.dob and profiles.startdate are free-text strings in Laravel ("12.12.1984", "8/11/2019"),
    /// with dob_date / startdate_date holding the parsed value (used for the medal-due notes).
    /// </summary>
    public static class DateUtils
    {
        private static readonly string[] Formats =
        {
            "d/M/yyyy", "d.M.yyyy", "d-M-yyyy", "yyyy-MM-dd", "d/M/yy", "d.M.yy"
        };

        public static DateTime? ParseAustralian(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            return DateTime.TryParseExact(text.Trim(), Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime d) ? d : null;
        }

        /// <summary>yyyy-MM-dd for an HTML date input, or "" when the text can't be parsed.</summary>
        public static string ToIso(string? text, DateTime? fallback)
        {
            DateTime? d = ParseAustralian(text) ?? fallback;
            return d?.ToString("yyyy-MM-dd") ?? "";
        }

        /// <summary>The text format new values are stored in, matching most existing rows.</summary>
        public static string ToAustralian(DateTime d) => d.ToString("dd/MM/yyyy");
    }
}
