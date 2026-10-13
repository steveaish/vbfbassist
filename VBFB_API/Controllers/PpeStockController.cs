using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using VBFB_API.Classes;
using VBFB_API.Models;

namespace VBFB_API.Controllers
{
    /// <summary>
    /// PPE issued to members (ppe_stock): issue / return / retire from the profile screen, and
    /// check / acknowledge from My Page. Every movement is written to ppe_history, as in Laravel.
    /// ppe_history from/to ids: a profile id, 0 = stock cupboard, -1 = retired.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PpeStockController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PpeStockController(ApplicationDbContext context)
        {
            _context = context;
        }

        public static List<Dictionary<string, object?>> ForProfile(ApplicationDbContext db, int profileId)
        {
            return SqlQuery.Select(db, @"select st.id, i.name as Item, s.name as Style, issuedate, returndate, laststocktakedate, notes,
                    st.deleted, returned, acknowledged
                from ppe_stock st
                inner join ppe_items i on st.itemid = i.id
                inner join ppe_Style s on st.styleid = s.id
                where profileid = @profileid
                order by case when st.deleted = 'N' and returned = 'N' then 0 else 1 end, returndate desc, i.name",
                new[] { new SqlParameter("@profileid", profileId) });
        }

        public static List<Dictionary<string, object?>> AvailableStyles(ApplicationDbContext db)
        {
            return SqlQuery.Select(db, @"select s.id, i.name as ItemName, s.name as StyleName, s.quantity
                from ppe_items i inner join ppe_Style s on i.id = s.ItemSeqno
                where i.deleted = 'N' and s.deleted = 'N'
                order by i.name, s.name");
        }

        // POST api/v1/ppestock/issue  { profileid, styleid }
        [HttpPost("issue")]
        [RequirePermission("isadmin")]
        public ActionResult Issue([FromBody] JObject data)
        {
            int profileId = (int?)data["profileid"] ?? 0;
            int styleId = (int?)data["styleid"] ?? 0;

            ppe_Style? style = _context.ppe_Style.FirstOrDefault(s => s.id == styleId);
            if (style == null || !_context.profiles.Any(p => p.id == profileId)) return BadRequest(new { message = "Unknown item or member." });

            _context.ppe_stock.Add(new ppe_stock
            {
                profileid = profileId,
                itemid = style.ItemSeqno,
                styleid = styleId,
                issuedate = DateTime.Now,
                deleted = "N",
                returned = "N",
                acknowledged = "N",
                notified = "N"
            });
            style.quantity = (style.quantity ?? 0) - 1;
            AddHistory(style.ItemSeqno, styleId, 0, profileId, 1, "Assigning Item");
            _context.SaveChanges();
            return Ok();
        }

        // POST api/v1/ppestock/5/return - back into the stock cupboard
        [HttpPost("{id:int}/return")]
        [RequirePermission("isadmin")]
        public ActionResult Return(int id)
        {
            ppe_stock? stock = _context.ppe_stock.FirstOrDefault(s => s.id == id);
            if (stock == null) return NotFound();

            stock.returndate = DateTime.Now;
            stock.returned = "Y";
            ppe_Style? style = _context.ppe_Style.FirstOrDefault(s => s.id == stock.styleid);
            if (style != null) style.quantity = (style.quantity ?? 0) + 1;
            AddHistory(stock.itemid, stock.styleid, stock.profileid, 0, 1, "Returning Item");
            _context.SaveChanges();
            return Ok();
        }

        // POST api/v1/ppestock/5/retire - worn out / disposed of, does not go back into stock
        [HttpPost("{id:int}/retire")]
        [RequirePermission("isadmin")]
        public ActionResult Retire(int id)
        {
            ppe_stock? stock = _context.ppe_stock.FirstOrDefault(s => s.id == id);
            if (stock == null) return NotFound();

            stock.returndate = DateTime.Now;
            stock.deleted = "Y";
            AddHistory(stock.itemid, stock.styleid, stock.profileid, -1, 1, "Retiring Item");
            _context.SaveChanges();
            return Ok();
        }

        // POST api/v1/ppestock/5/check - the member confirms they still have the item
        [HttpPost("{id:int}/check")]
        public ActionResult Check(int id) => MemberAction(id, acknowledge: false);

        // POST api/v1/ppestock/5/acknowledge - the member confirms they received the item
        [HttpPost("{id:int}/acknowledge")]
        public ActionResult Acknowledge(int id) => MemberAction(id, acknowledge: true);

        private ActionResult MemberAction(int id, bool acknowledge)
        {
            ppe_stock? stock = _context.ppe_stock.FirstOrDefault(s => s.id == id);
            if (stock == null) return NotFound();

            // Laravel let anyone check any item; only the holder (or an admin) may now
            profiles? me = Permissions.CurrentProfile(HttpContext, _context);
            if (stock.profileid != me?.id && !Permissions.Has(HttpContext, _context, "isadmin")) return StatusCode(StatusCodes.Status403Forbidden);

            stock.laststocktakedate = DateTime.Now;
            if (acknowledge) stock.acknowledged = "Y";
            AddHistory(stock.itemid, stock.styleid, stock.profileid, stock.profileid, 0, acknowledge ? "Acknowledge Receipt" : "PPE Check");
            _context.SaveChanges();
            return Ok();
        }

        private void AddHistory(int? itemId, int? styleId, int? fromId, int? toId, int quantity, string notes)
        {
            _context.ppe_history.Add(new ppe_history
            {
                historydate = DateTime.Now,
                loggedinid = Permissions.CurrentProfile(HttpContext, _context)?.id,
                itemid = itemId,
                styleid = styleId,
                fromid = fromId,
                toid = toId,
                Quantity = quantity,
                notes = notes
            });
        }
    }
}
