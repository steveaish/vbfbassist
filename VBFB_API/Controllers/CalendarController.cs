using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Globalization;
using VBFB_API.Classes;
using VBFB_API.Models;

namespace VBFB_API.Controllers
{
    /// <summary>
    /// Brigade calendar (Laravel CalendarController). Everyone can view; Can Edit Calendar can change.
    /// calendar.thetime / endtime hold only a time (on 1900-01-01). Repeating events are separate rows
    /// sharing a repeatguid, so a series can be edited "from here on" or deleted together.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CalendarController : ControllerBase
    {
        private const int MaxRepeats = 104;
        private static readonly DateTime TimeBase = new(1900, 1, 1);

        private readonly ApplicationDbContext _context;

        public CalendarController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("opened")]
        public ActionResult Opened()
        {
            ProgramLog.Write(HttpContext, _context, ProgramLog.Calendar, "Open Calendar Screen");
            return Ok();
        }

        // GET api/v1/calendar?year=2026&month=10
        [HttpGet]
        public ActionResult<dynamic> Month([FromQuery] int year, [FromQuery] int month)
        {
            if (year < 1900 || month < 1 || month > 12) return BadRequest(new { message = "Invalid month." });
            DateTime from = new(year, month, 1);
            DateTime to = from.AddMonths(1);

            var items = _context.calendar.AsNoTracking()
                .Where(c => c.thedate >= from && c.thedate < to)
                .OrderBy(c => c.thedate!.Value.Date).ThenBy(c => c.thetime).ThenBy(c => c.endtime)
                .ToList()
                .Select(ToDto);

            return Ok(new { items, canedit = Permissions.Has(HttpContext, _context, "caneditcalendar") });
        }

        // GET api/v1/calendar/5
        [HttpGet("{id:int}")]
        public ActionResult<dynamic> Get(int id)
        {
            calendar? c = _context.calendar.AsNoTracking().FirstOrDefault(x => x.id == id);
            if (c == null) return NotFound();
            int seriesCount = string.IsNullOrEmpty(c.repeatguid) ? 1 : _context.calendar.Count(x => x.repeatguid == c.repeatguid);
            return Ok(new { item = ToDto(c), seriesCount });
        }

        // POST api/v1/calendar { date, start, end, description, longdesc, colour, repeat: None|Day|Week|Month, every, count }
        [HttpPost]
        [RequirePermission("caneditcalendar")]
        public ActionResult<dynamic> Create([FromBody] JObject data)
        {
            if (!TryRead(data, out calendar first, out string? error)) return BadRequest(new { message = error });

            string repeat = (string?)data["repeat"] ?? "None";
            int every = Math.Max(1, (int?)data["every"] ?? 1);
            int count = Math.Clamp((int?)data["count"] ?? 0, 0, MaxRepeats);

            List<calendar> rows = new() { first };
            if (repeat is "Day" or "Week" or "Month" && count > 0)
            {
                string guid = Guid.NewGuid().ToString().ToUpperInvariant();
                first.repeatguid = guid;
                DateTime start = first.thedate!.Value;
                for (int i = 1; i <= count; i++)
                {
                    // Count from the first date each time so month-ends don't drift (Jan 31 -> Feb 28 -> Mar 31)
                    DateTime d = repeat switch
                    {
                        "Day" => start.AddDays(every * i),
                        "Week" => start.AddDays(7 * every * i),
                        _ => start.AddMonths(every * i)
                    };
                    rows.Add(new calendar
                    {
                        thedate = d,
                        thetime = first.thetime,
                        endtime = first.endtime,
                        description = first.description,
                        longdesc = first.longdesc,
                        colour = first.colour,
                        repeatguid = guid
                    });
                }
            }

            _context.calendar.AddRange(rows);
            _context.SaveChanges();
            return Ok(new { id = first.id, added = rows.Count });
        }

        // PUT api/v1/calendar/5 { date, start, end, description, longdesc, colour, applytofollowing }
        // applytofollowing copies the time/colour/text (not the date) to later events in the series, as Laravel's "save all"
        [HttpPut("{id:int}")]
        [RequirePermission("caneditcalendar")]
        public ActionResult<dynamic> Update(int id, [FromBody] JObject data)
        {
            calendar? c = _context.calendar.FirstOrDefault(x => x.id == id);
            if (c == null) return NotFound();
            if (!TryRead(data, out calendar values, out string? error)) return BadRequest(new { message = error });

            c.thedate = values.thedate;
            Copy(values, c);

            int following = 0;
            if ((bool?)data["applytofollowing"] == true && !string.IsNullOrEmpty(c.repeatguid))
            {
                foreach (calendar later in _context.calendar.Where(x => x.repeatguid == c.repeatguid && x.thedate > c.thedate && x.id != c.id))
                {
                    Copy(values, later);
                    following++;
                }
            }
            _context.SaveChanges();
            return Ok(new { following });
        }

        // DELETE api/v1/calendar/5?series=Y
        [HttpDelete("{id:int}")]
        [RequirePermission("caneditcalendar")]
        public ActionResult<dynamic> Delete(int id, [FromQuery] string? series)
        {
            calendar? c = _context.calendar.FirstOrDefault(x => x.id == id);
            if (c == null) return NotFound();

            List<calendar> rows = series == "Y" && !string.IsNullOrEmpty(c.repeatguid)
                ? _context.calendar.Where(x => x.repeatguid == c.repeatguid).ToList()
                : new List<calendar> { c };
            _context.calendar.RemoveRange(rows);
            _context.SaveChanges();
            return Ok(new { deleted = rows.Count });
        }

        private static void Copy(calendar from, calendar to)
        {
            to.thetime = from.thetime;
            to.endtime = from.endtime;
            to.description = from.description;
            to.longdesc = from.longdesc;
            to.colour = from.colour;
        }

        private static bool TryRead(JObject data, out calendar c, out string? error)
        {
            c = new calendar();
            error = null;

            if (!DateTime.TryParseExact((string?)data["date"], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                error = "Please choose a date.";
                return false;
            }
            string? description = ((string?)data["description"])?.Trim();
            if (string.IsNullOrEmpty(description))
            {
                error = "Please enter a label.";
                return false;
            }

            c.thedate = date;
            c.thetime = ParseTime((string?)data["start"]) ?? TimeBase;
            c.endtime = ParseTime((string?)data["end"]);
            c.description = description;
            c.longdesc = (string?)data["longdesc"];
            string? colour = (string?)data["colour"];
            c.colour = colour != null && System.Text.RegularExpressions.Regex.IsMatch(colour, "^#[0-9a-fA-F]{6}$") ? colour.ToLowerInvariant() : "#ffffff";
            return true;
        }

        private static DateTime? ParseTime(string? hhmm) =>
            TimeSpan.TryParseExact(hhmm, @"hh\:mm", CultureInfo.InvariantCulture, out TimeSpan t) ? TimeBase.Add(t) : null;

        private static object ToDto(calendar c) => new
        {
            c.id,
            date = c.thedate?.ToString("yyyy-MM-dd"),
            start = c.thetime?.ToString("HH:mm"),
            end = c.endtime?.ToString("HH:mm"),
            c.description,
            c.longdesc,
            colour = string.IsNullOrEmpty(c.colour) ? "#ffffff" : c.colour,
            repeating = !string.IsNullOrEmpty(c.repeatguid)
        };
    }
}
