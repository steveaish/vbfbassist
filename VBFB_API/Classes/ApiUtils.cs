using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace VBFB_API.Classes
{
    public class ApiBreakDown
    {
        public string where { get; set; } = "";
        public string orderby { get; set; } = "";
        public string skipandfetch { get; set; } = "";
        public List<SqlParameter> parameters { get; set; } = new();
    }

    public static partial class ApiUtils
    {
        [GeneratedRegex(@"^[A-Za-z_][A-Za-z0-9_\.]*$")]
        private static partial Regex IdentifierRegex();

        /// <summary>
        /// Turns a Syncfusion DataManager (UrlAdaptor) POST body into SQL fragments, the same contract as
        /// ReadyApi's SyncFusionBreakDownApiBody: "where" starts with "and ...", "orderby" has no ORDER BY
        /// keyword, "skipandfetch" is an OFFSET/FETCH clause. Field names must be plain identifiers;
        /// values always go through parameters.
        /// </summary>
        public static ApiBreakDown SyncFusionBreakDownApiBody(JObject? data, string defaultOrderBy)
        {
            ApiBreakDown result = new() { orderby = defaultOrderBy };
            if (data == null) return result;

            if (data["sorted"] is JArray sorted && sorted.Count > 0)
            {
                List<string> parts = new();
                // Syncfusion sends the most recently clicked column first; SQL wants it first too
                foreach (JToken s in sorted)
                {
                    string? name = (string?)s["name"];
                    if (name == null || !IdentifierRegex().IsMatch(name)) continue;
                    parts.Add(name + ((string?)s["direction"] == "descending" ? " desc" : ""));
                }
                if (parts.Count > 0) result.orderby = string.Join(", ", parts);
            }

            if (data["where"] is JArray where)
            {
                List<string> clauses = new();
                foreach (JToken w in where) AddPredicate(w, clauses, result.parameters);
                if (clauses.Count > 0) result.where = "and " + string.Join(" and ", clauses);
            }

            if (data["take"] != null && (int?)data["take"] > 0)
            {
                result.skipandfetch = $" offset {(int?)data["skip"] ?? 0} rows fetch next {(int)data["take"]!} rows only ";
            }

            return result;
        }

        // Recursively handles grouped predicates (isComplex + condition and/or) as well as single ones
        private static void AddPredicate(JToken p, List<string> clauses, List<SqlParameter> parameters)
        {
            if ((bool?)p["isComplex"] == true && p["predicates"] is JArray children)
            {
                List<string> inner = new();
                foreach (JToken c in children) AddPredicate(c, inner, parameters);
                if (inner.Count > 0)
                {
                    string joiner = ((string?)p["condition"])?.ToLower() == "or" ? " or " : " and ";
                    clauses.Add("(" + string.Join(joiner, inner) + ")");
                }
                return;
            }

            string? field = (string?)p["field"];
            if (field == null || !IdentifierRegex().IsMatch(field)) return;

            string pname = "@p" + parameters.Count;
            object value = ((JValue?)p["value"])?.Value ?? DBNull.Value;
            string op = ((string?)p["operator"])?.ToLower() ?? "equal";

            switch (op)
            {
                case "startswith":
                    clauses.Add($"{field} like {pname} + '%'");
                    break;
                case "endswith":
                    clauses.Add($"{field} like '%' + {pname}");
                    break;
                case "contains":
                    clauses.Add($"{field} like '%' + {pname} + '%'");
                    break;
                case "notequal":
                    clauses.Add($"{field} <> {pname}");
                    break;
                case "greaterthan":
                    clauses.Add($"{field} > {pname}");
                    break;
                case "greaterthanorequal":
                    clauses.Add($"{field} >= {pname}");
                    break;
                case "lessthan":
                    clauses.Add($"{field} < {pname}");
                    break;
                case "lessthanorequal":
                    clauses.Add($"{field} <= {pname}");
                    break;
                default:
                    clauses.Add($"{field} = {pname}");
                    break;
            }
            parameters.Add(new SqlParameter(pname, value));
        }

        /// <summary>The logged-in user's id from the UserId header (validated by the middleware).</summary>
        public static int GetUserId(HttpContext context)
        {
            return context.Items["UserId"] is int id ? id : 0;
        }
    }
}
