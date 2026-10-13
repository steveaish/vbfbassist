using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using VBFB_API.Classes;
using VBFB_API.Models;

namespace VBFB_API.Controllers
{
    /// <summary>
    /// PPE stock cupboard (Laravel PPEController items/styles): items, their styles (sizes) with the
    /// quantity in stock, stock adjustments and the movement history. Soft-deleted, as Laravel.
    /// Issuing to members is PpeStockController.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [RequirePermission("isadmin")]
    public class PpeItemsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PpeItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("opened")]
        public ActionResult Opened()
        {
            ProgramLog.Write(HttpContext, _context, ProgramLog.Vehicles, "Open PPE Items Edit Screen");
            return Ok();
        }

        // GET api/v1/ppeitems - items with total in stock and number currently issued
        [HttpGet]
        public ActionResult<dynamic> List()
        {
            return Ok(SqlQuery.Select(_context, @"select i.id, i.Name,
                    isnull((select sum(quantity) from ppe_Style where ItemSeqno = i.id and deleted = 'N'), 0) as InStock,
                    (select count(*) from ppe_stock where itemid = i.id and deleted = 'N' and returned = 'N') as Issued
                from ppe_items i
                where i.deleted = 'N'
                order by i.Name"));
        }

        [HttpPost]
        public ActionResult<dynamic> Create([FromBody] JObject data)
        {
            string? name = ((string?)data["Name"])?.Trim();
            if (string.IsNullOrEmpty(name)) return BadRequest(new { message = "Please enter a name." });
            ppe_items item = new() { Name = name, deleted = "N" };
            _context.ppe_items.Add(item);
            _context.SaveChanges();
            return Ok(new { item.id });
        }

        [HttpPut("{id:int}")]
        public ActionResult Update(int id, [FromBody] JObject data)
        {
            ppe_items? item = _context.ppe_items.FirstOrDefault(i => i.id == id);
            string? name = ((string?)data["Name"])?.Trim();
            if (item == null) return NotFound();
            if (string.IsNullOrEmpty(name)) return BadRequest(new { message = "Please enter a name." });
            item.Name = name;
            _context.SaveChanges();
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            ppe_items? item = _context.ppe_items.FirstOrDefault(i => i.id == id);
            if (item == null) return NotFound();
            item.deleted = "Y";
            _context.SaveChanges();
            return Ok();
        }

        // GET api/v1/ppeitems/5 - the item and its styles
        [HttpGet("{id:int}")]
        public ActionResult<dynamic> Get(int id)
        {
            ppe_items? item = _context.ppe_items.AsNoTracking().FirstOrDefault(i => i.id == id && i.deleted == "N");
            if (item == null) return NotFound();

            return Ok(new
            {
                item = new { item.id, item.Name },
                styles = SqlQuery.Select(_context, @"select s.id, s.Name, isnull(s.quantity, 0) as quantity,
                        (select count(*) from ppe_stock where styleid = s.id and deleted = 'N' and returned = 'N') as Issued
                    from ppe_Style s
                    where s.ItemSeqno = @id and s.deleted = 'N'
                    order by s.Name", new[] { new SqlParameter("@id", id) })
            });
        }

        // GET api/v1/ppeitems/5/history
        [HttpGet("{id:int}/history")]
        public ActionResult<dynamic> History(int id)
        {
            return Ok(SqlQuery.Select(_context, @"select h.id, h.historydate, concat(p.firstname, ' ', p.lastname) as Profile, s.Name as StyleName,
                    case when h.fromid = 0 then 'Stock Cupboard' when h.fromid = -1 then 'Retired' else concat(fromloc.firstname, ' ', fromloc.lastname) end as FromLocation,
                    case when h.toid = 0 then 'Stock Cupboard' when h.toid = -1 then 'Retired' else concat(toloc.firstname, ' ', toloc.lastname) end as ToLocation,
                    h.Quantity, h.notes
                from ppe_history h
                left join profiles p on h.loggedinid = p.id
                left join ppe_Style s on h.styleid = s.id
                left join profiles fromloc on h.fromid = fromloc.id
                left join profiles toloc on h.toid = toloc.id
                where h.itemid = @id
                order by h.historydate desc, h.id desc", new[] { new SqlParameter("@id", id) }));
        }

        // POST api/v1/ppeitems/5/styles { Name, quantity }
        [HttpPost("{id:int}/styles")]
        public ActionResult AddStyle(int id, [FromBody] JObject data)
        {
            string? name = ((string?)data["Name"])?.Trim();
            if (string.IsNullOrEmpty(name)) return BadRequest(new { message = "Please enter a style name." });
            if (!_context.ppe_items.Any(i => i.id == id)) return NotFound();

            double qty = (double?)data["quantity"] ?? 0;
            ppe_Style style = new() { ItemSeqno = id, Name = name, quantity = qty, deleted = "N" };
            _context.ppe_Style.Add(style);
            _context.SaveChanges();
            if (qty != 0) AddHistory(id, style.id, qty);
            _context.SaveChanges();
            return Ok();
        }

        // PUT api/v1/ppeitems/styles/9 { Name }
        [HttpPut("styles/{styleId:int}")]
        public ActionResult UpdateStyle(int styleId, [FromBody] JObject data)
        {
            ppe_Style? style = _context.ppe_Style.FirstOrDefault(s => s.id == styleId);
            string? name = ((string?)data["Name"])?.Trim();
            if (style == null) return NotFound();
            if (string.IsNullOrEmpty(name)) return BadRequest(new { message = "Please enter a style name." });
            style.Name = name;
            _context.SaveChanges();
            return Ok();
        }

        [HttpDelete("styles/{styleId:int}")]
        public ActionResult DeleteStyle(int styleId)
        {
            ppe_Style? style = _context.ppe_Style.FirstOrDefault(s => s.id == styleId);
            if (style == null) return NotFound();
            style.deleted = "Y";
            _context.SaveChanges();
            return Ok();
        }

        // POST api/v1/ppeitems/styles/9/quantity { quantity } - stocktake: set the count, log the difference
        [HttpPost("styles/{styleId:int}/quantity")]
        public ActionResult SetQuantity(int styleId, [FromBody] JObject data)
        {
            ppe_Style? style = _context.ppe_Style.FirstOrDefault(s => s.id == styleId);
            if (style == null) return NotFound();
            double? qty = (double?)data["quantity"];
            if (qty == null || qty < 0) return BadRequest(new { message = "Please enter a quantity of 0 or more." });

            double change = qty.Value - (style.quantity ?? 0);
            if (change == 0) return Ok();
            style.quantity = qty;
            AddHistory(style.ItemSeqno, styleId, change);
            _context.SaveChanges();
            return Ok();
        }

        private void AddHistory(int? itemId, int styleId, double change)
        {
            _context.ppe_history.Add(new ppe_history
            {
                historydate = DateTime.Now,
                loggedinid = Permissions.CurrentProfile(HttpContext, _context)?.id,
                itemid = itemId,
                styleid = styleId,
                fromid = 0,
                toid = 0,
                Quantity = change,
                notes = change >= 0 ? $"Added {change} items" : $"Subtracted {-change} items"
            });
        }
    }
}
