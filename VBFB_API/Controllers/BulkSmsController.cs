using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using VBFB_API.Classes;
using VBFB_API.Models;

namespace VBFB_API.Controllers
{
    /// <summary>
    /// Bulk SMS (Laravel BulkSMSController): a message (sms_message) and its recipients (sms_list),
    /// sent as email to &lt;mobile&gt;@sms.clicksend.com via SMTP, as Laravel did. A sent message is locked.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [RequirePermission("canbulksms")]
    public partial class BulkSmsController : ControllerBase
    {
        // Membership filter -> profiles flag column (Laravel's "Auxilliary Member" used a column that doesn't exist)
        private static readonly Dictionary<string, string> Filters = new()
        {
            ["FirstLine"] = "firstlinefirefighter",
            ["SecondLine"] = "secondlinefirefighter",
            ["President"] = "president",
            ["Captain"] = "captain",
            ["Lieutenant"] = "lieutenant",
            ["OtherOfficer"] = "otherofficer",
            ["Cadet"] = "cadet",
            ["CadetLeader"] = "cadetleader",
            ["LifeMember"] = "lifemember",
            ["AuxMember"] = "auxmember"
        };

        // ClickSend/SMTP2GO cope better with modest recipient lists per email
        private const int RecipientsPerEmail = 50;

        private readonly ApplicationDbContext _context;

        public BulkSmsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("opened")]
        public ActionResult Opened()
        {
            ProgramLog.Write(HttpContext, _context, ProgramLog.BulkSms, "Open Bulk SMS Screen");
            return Ok();
        }

        // GET api/v1/bulksms
        [HttpGet]
        public ActionResult<dynamic> List()
        {
            return Ok(SqlQuery.Select(_context, @"select m.seqno, m.message, m.sent, m.senttime, m.createddate,
                    (select count(*) from sms_list where messageid = m.seqno) as Recipients
                from sms_message m order by m.seqno desc"));
        }

        // POST api/v1/bulksms { message }
        [HttpPost]
        public ActionResult<dynamic> Create([FromBody] JObject data)
        {
            string? text = ((string?)data["message"])?.Trim();
            if (string.IsNullOrEmpty(text)) return BadRequest(new { message = "Please enter a message." });
            sms_message m = new() { message = text, sent = "N", createddate = DateTime.Now };
            _context.sms_message.Add(m);
            _context.SaveChanges();
            return Ok(new { id = m.seqno });
        }

        // GET api/v1/bulksms/5?filter=FirstLine - the message, its recipients and the members who could be added
        [HttpGet("{id:int}")]
        public ActionResult<dynamic> Get(int id, [FromQuery] string? filter)
        {
            sms_message? m = _context.sms_message.AsNoTracking().FirstOrDefault(x => x.seqno == id);
            if (m == null) return NotFound();
            SqlParameter[] p = { new("@id", id) };

            return Ok(new
            {
                message = new { m.seqno, m.message, m.sent, m.senttime, m.createddate },
                recipients = SqlQuery.Select(_context, @"select l.seqno, p.id as profileid, p.firstname, p.lastname, p.mobilephone
                    from sms_list l inner join profiles p on l.profileid = p.id
                    where l.messageid = @id order by p.firstname, p.lastname", p),
                available = SqlQuery.Select(_context, $@"select p.id, p.firstname, p.lastname, p.mobilephone
                    from profiles p
                    where {AvailableWhere(filter)}
                    order by p.firstname, p.lastname", p),
                filters = Filters.Keys
            });
        }

        // Active members with a mobile who aren't already on the list (Laravel also included inactive members)
        private static string AvailableWhere(string? filter)
        {
            string where = "p.isactive = 'Y' and isnull(p.mobilephone, '') <> '' and p.id not in (select profileid from sms_list where messageid = @id)";
            if (filter != null && Filters.TryGetValue(filter, out string? column)) where += $" and p.{column} = 'Y'";
            return where;
        }

        // PUT api/v1/bulksms/5 { message }
        [HttpPut("{id:int}")]
        public ActionResult Update(int id, [FromBody] JObject data)
        {
            sms_message? m = _context.sms_message.FirstOrDefault(x => x.seqno == id);
            if (m == null) return NotFound();
            if (m.sent == "Y") return BadRequest(new { message = "This message has already been sent." });
            string? text = ((string?)data["message"])?.Trim();
            if (string.IsNullOrEmpty(text)) return BadRequest(new { message = "Please enter a message." });
            m.message = text;
            _context.SaveChanges();
            return Ok();
        }

        // DELETE api/v1/bulksms/5 - and its recipient rows (Laravel left them behind)
        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            sms_message? m = _context.sms_message.FirstOrDefault(x => x.seqno == id);
            if (m == null) return NotFound();
            _context.sms_list.RemoveRange(_context.sms_list.Where(l => l.messageid == id));
            _context.sms_message.Remove(m);
            _context.SaveChanges();
            return Ok();
        }

        // POST api/v1/bulksms/5/recipients { profileids: [..] } or { all: true, filter }
        [HttpPost("{id:int}/recipients")]
        public ActionResult AddRecipients(int id, [FromBody] JObject data)
        {
            sms_message? m = _context.sms_message.AsNoTracking().FirstOrDefault(x => x.seqno == id);
            if (m == null) return NotFound();
            if (m.sent == "Y") return BadRequest(new { message = "This message has already been sent." });

            List<int> ids;
            if ((bool?)data["all"] == true)
            {
                ids = SqlQuery.Select(_context, $"select p.id from profiles p where {AvailableWhere((string?)data["filter"])}", new[] { new SqlParameter("@id", id) })
                    .Select(r => Convert.ToInt32(r["id"])).ToList();
            }
            else
            {
                HashSet<int> already = _context.sms_list.Where(l => l.messageid == id).Select(l => l.profileid ?? 0).ToHashSet();
                ids = (data["profileids"] as JArray ?? new JArray()).Select(t => (int)t).Where(i => !already.Contains(i)).Distinct().ToList();
            }

            foreach (int pid in ids)
            {
                _context.sms_list.Add(new sms_list { messageid = id, profileid = pid, status = 0, posttime = DateTime.Now });
            }
            _context.SaveChanges();
            return Ok(new { added = ids.Count });
        }

        // DELETE api/v1/bulksms/recipients/77
        [HttpDelete("recipients/{seqno:int}")]
        public ActionResult RemoveRecipient(int seqno)
        {
            sms_list? row = _context.sms_list.FirstOrDefault(l => l.seqno == seqno);
            if (row == null) return NotFound();
            if (_context.sms_message.Any(m => m.seqno == row.messageid && m.sent == "Y")) return BadRequest(new { message = "This message has already been sent." });
            _context.sms_list.Remove(row);
            _context.SaveChanges();
            return Ok();
        }

        [GeneratedRegex(@"[^\d+]")]
        private static partial Regex NonPhoneChars();

        // POST api/v1/bulksms/5/send
        [HttpPost("{id:int}/send")]
        public ActionResult<dynamic> Send(int id)
        {
            sms_message? m = _context.sms_message.FirstOrDefault(x => x.seqno == id);
            if (m == null) return NotFound();
            if (m.sent == "Y") return BadRequest(new { message = "This message has already been sent." });

            List<sms_list> rows = _context.sms_list.Where(l => l.messageid == id).ToList();
            Dictionary<int, string?> mobiles = _context.profiles.AsNoTracking()
                .Where(p => rows.Select(r => r.profileid).Contains(p.id))
                .ToDictionary(p => p.id, p => p.mobilephone);

            List<(sms_list row, string address)> targets = rows
                .Select(r => (r, mobile: NonPhoneChars().Replace(mobiles.GetValueOrDefault(r.profileid ?? 0) ?? "", "")))
                .Where(x => x.mobile.Length >= 8)
                .Select(x => (x.r, $"{x.mobile}@{Globals.Ready.SmsEmailDomain}"))
                .ToList();
            if (targets.Count == 0) return BadRequest(new { message = "There are no recipients with a mobile number." });

            int sent = 0;
            try
            {
                foreach (var batch in targets.Chunk(RecipientsPerEmail))
                {
                    Mailer.Send(batch.Select(b => b.address), "", m.message ?? "", fromAddress: Globals.Ready.SmsFromAddress);
                    foreach (var (row, _) in batch)
                    {
                        row.status = 1;
                        row.senttime = DateTime.Now;
                    }
                    sent += batch.Length;
                }
            }
            catch (Exception ex)
            {
                Logging.Log(Logging.LogLevel.Error, $"Bulk SMS {id} failed after {sent} recipients: {ex.Message}");
                _context.SaveChanges();
                return BadRequest(new { message = $"Sending failed after {sent} of {targets.Count} recipients: {ex.Message}" });
            }

            m.sent = "Y";
            m.senttime = DateTime.Now;
            _context.SaveChanges();
            return Ok(new { sent });
        }
    }
}
