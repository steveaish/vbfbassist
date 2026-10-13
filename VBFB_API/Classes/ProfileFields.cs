using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using VBFB_API.Models;

namespace VBFB_API.Classes
{
    /// <summary>
    /// Shared profile pieces: which fields each screen may write, the medal-due SQL, and the
    /// list filters (search / inactive / types / crews) used by the Profiles and Members grids.
    /// </summary>
    public static class ProfileFields
    {
        // Laravel Profiles::$fillable, minus dob and startdate which go through ApplyDates
        public static readonly string[] AdminFillable =
        {
            "firstname", "lastname", "isactive", "email", "isadmin", "isfacilitator", "iscoordinator", "isstreetcoordinator",
            "homephone", "mobilephone", "address", "dlicense", "dfesnumber", "bartactive", "prefcomm", "crew", "sex",
            "firstlinefirefighter", "secondlinefirefighter", "president", "captain", "lieutenant", "otherofficer", "cadet", "lifemember", "auxmember",
            "caregivername", "cadetcrew", "photossocialmedia", "caregivermobile", "caregiverhome", "caregiverwork", "cananyoneelsepickup",
            "whocanpickup", "whocannotpickup", "allergies", "allergydetails", "medicalyn", "medicaldetails", "cadetleader", "IsVehicleMaint",
            "workphone", "comments", "ViewMembers", "canbulksms", "BFRResident", "streetid", "year5", "year10", "year15", "year20", "year25", "year30",
            "caneditcalendar", "signoffcode", "caneditdocuments"
        };

        // Legacy login columns on profiles - never sent to the browser
        private static readonly HashSet<string> Hidden = new(StringComparer.OrdinalIgnoreCase) { "username", "password" };

        // "p.firstname, p.lastname, ..." - every profiles column except the hidden ones
        public static readonly string SafeColumns = string.Join(", ",
            typeof(profiles).GetProperties().Where(p => !Hidden.Contains(p.Name)).Select(p => "p." + p.Name));

        public static Dictionary<string, object?> ToSafeDictionary(profiles profile) =>
            typeof(profiles).GetProperties().Where(p => !Hidden.Contains(p.Name)).ToDictionary(p => p.Name, p => p.GetValue(profile));

        // Fields on the cadet edit screen
        public static readonly string[] CadetFillable =
        {
            "firstname", "lastname", "email", "cadetcrew", "caregivername", "caregivermobile", "caregiverhome", "caregiverwork",
            "photossocialmedia", "cananyoneelsepickup", "whocanpickup", "whocannotpickup", "allergies", "allergydetails", "medicalyn", "medicaldetails"
        };

        // Fields members may see about each other (Laravel members/view: no address, DOB or medical)
        public const string MemberViewColumns =
            "p.id, firstname, lastname, email, sex, isactive, homephone, mobilephone, dlicense, dfesnumber, bartactive, prefcomm, crew, c.CrewName, alltypes, " +
            "isadmin, IsVehicleMaint, cadetleader, isfacilitator, iscoordinator, " +
            "firstlinefirefighter, secondlinefirefighter, president, captain, lieutenant, otherofficer, cadet, lifemember, auxmember";

        // Laravel's medal-due note on the profiles list
        public const string MedalNotesSql = @"case
            when DATEDIFF(year, startdate_date, getdate()) > 29.8 and year30 = 'N' then '30 year medal due ' + convert(varchar, DateAdd(year, 30, startdate_date), 3)
            when DATEDIFF(year, startdate_date, getdate()) > 24.8 and year25 = 'N' then '25 year medal due ' + convert(varchar, DateAdd(year, 25, startdate_date), 3)
            when DATEDIFF(year, startdate_date, getdate()) > 19.8 and year20 = 'N' then '20 year medal due ' + convert(varchar, DateAdd(year, 20, startdate_date), 3)
            when DATEDIFF(year, startdate_date, getdate()) > 14.8 and year15 = 'N' then '15 year medal due ' + convert(varchar, DateAdd(year, 15, startdate_date), 3)
            when DATEDIFF(year, startdate_date, getdate()) > 9.8 and year10 = 'N' then '10 year medal due ' + convert(varchar, DateAdd(year, 10, startdate_date), 3)
            when DATEDIFF(year, startdate_date, getdate()) > 4.8 and year5 = 'N' then '5 year medal due ' + convert(varchar, DateAdd(year, 5, startdate_date), 3)
            end";

        // Type filter: the Laravel profile list's "typesearch" options (flag column -> any of them = 'Y')
        private static readonly HashSet<string> TypeColumns = new(StringComparer.OrdinalIgnoreCase)
        {
            "isadmin", "isfacilitator", "iscoordinator", "isstreetcoordinator", "firstlinefirefighter", "secondlinefirefighter",
            "president", "captain", "lieutenant", "cadet", "otherofficer", "cadetleader", "lifemember", "auxmember"
        };

        /// <summary>
        /// Extra where-clause from the grid's addParams: search, showinactive (Y), types[], crews[].
        /// Syncfusion's UrlAdaptor sends addParams under "params".
        /// </summary>
        public static string BuildFilters(JObject? data, List<SqlParameter> parameters)
        {
            JObject? p = data?["params"] as JObject;
            List<string> clauses = new();

            string? search = (string?)p?["search"];
            if (!string.IsNullOrWhiteSpace(search))
            {
                parameters.Add(new SqlParameter("@search", search.Trim()));
                clauses.Add("(firstname like '%' + @search + '%' or lastname like '%' + @search + '%' or (firstname + ' ' + lastname) like '%' + @search + '%' " +
                            "or homephone like '%' + @search + '%' or mobilephone like '%' + @search + '%')");
            }

            if ((string?)p?["showinactive"] != "Y") clauses.Add("isactive = 'Y'");

            if (p?["types"] is JArray types)
            {
                List<string> typeClauses = types.Select(t => (string?)t).Where(t => t != null && TypeColumns.Contains(t)).Select(t => $"p.{t} = 'Y'").ToList();
                if (typeClauses.Count > 0) clauses.Add("(" + string.Join(" or ", typeClauses) + ")");
            }

            if (p?["crews"] is JArray crews)
            {
                List<int> crewIds = crews.Select(c => int.TryParse(c.ToString(), out int i) ? i : (int?)null).Where(i => i != null).Select(i => i!.Value).ToList();
                if (crewIds.Count > 0) clauses.Add("p.crew in (" + string.Join(",", crewIds) + ")");
            }

            return clauses.Count == 0 ? "" : "and " + string.Join(" and ", clauses);
        }

        /// <summary>Applies dob/startdate (sent as yyyy-MM-dd) to both the text and the _date column.</summary>
        public static void ApplyDates(profiles profile, JObject data)
        {
            if (data.ContainsKey("dob"))
            {
                DateTime? d = DateUtils.ParseAustralian((string?)data["dob"]);
                profile.dob = d == null ? ((string?)data["dob"] ?? "") : DateUtils.ToAustralian(d.Value);
                profile.dob_date = d;
            }
            if (data.ContainsKey("startdate"))
            {
                DateTime? d = DateUtils.ParseAustralian((string?)data["startdate"]);
                profile.startdate = d == null ? ((string?)data["startdate"] ?? "") : DateUtils.ToAustralian(d.Value);
                profile.startdate_date = d;
            }
        }
    }
}
