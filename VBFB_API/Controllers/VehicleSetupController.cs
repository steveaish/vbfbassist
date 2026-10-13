using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using VBFB_API.Classes;
using VBFB_API.Models;

namespace VBFB_API.Controllers
{
    /// <summary>
    /// Vehicle checklist setup (Laravel /veh/...): the master lists of sections and items, vehicles,
    /// and each vehicle's ordered sections (VEH_Veh_Sec) and items within them (VEH_Sec_Itm, with
    /// weekly / monthly flags). Everything is soft-deleted (deleted = 'Y') because old checks refer to it.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [RequirePermission("isadmin")]
    public class VehicleSetupController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VehicleSetupController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---- Master lists: sections and items (ListEditor screens) ----

        [HttpGet("sections")]
        public ActionResult<dynamic> Sections() =>
            Ok(_context.VEH_Sections.AsNoTracking().Where(s => s.deleted == "N").OrderBy(s => s.Name).Select(s => new { s.id, s.Name }).ToList());

        [HttpPost("sections")]
        public ActionResult AddSection([FromBody] JObject data)
        {
            string? name = Name(data);
            if (name == null) return BadRequest(new { message = "Please enter a name." });
            _context.VEH_Sections.Add(new VEH_Sections { Name = name, deleted = "N" });
            _context.SaveChanges();
            return Ok();
        }

        [HttpPut("sections/{id:int}")]
        public ActionResult UpdateSection(int id, [FromBody] JObject data)
        {
            VEH_Sections? s = _context.VEH_Sections.FirstOrDefault(x => x.id == id);
            string? name = Name(data);
            if (s == null) return NotFound();
            if (name == null) return BadRequest(new { message = "Please enter a name." });
            s.Name = name;
            _context.SaveChanges();
            return Ok();
        }

        [HttpDelete("sections/{id:int}")]
        public ActionResult DeleteSection(int id)
        {
            VEH_Sections? s = _context.VEH_Sections.FirstOrDefault(x => x.id == id);
            if (s == null) return NotFound();
            s.deleted = "Y";
            _context.SaveChanges();
            return Ok();
        }

        [HttpGet("items")]
        public ActionResult<dynamic> Items() =>
            Ok(_context.VEH_Items.AsNoTracking().Where(s => s.deleted == "N").OrderBy(s => s.Name).Select(s => new { s.id, s.Name }).ToList());

        [HttpPost("items")]
        public ActionResult AddItem([FromBody] JObject data)
        {
            string? name = Name(data);
            if (name == null) return BadRequest(new { message = "Please enter a name." });
            _context.VEH_Items.Add(new VEH_Items { Name = name, deleted = "N" });
            _context.SaveChanges();
            return Ok();
        }

        [HttpPut("items/{id:int}")]
        public ActionResult UpdateItem(int id, [FromBody] JObject data)
        {
            VEH_Items? s = _context.VEH_Items.FirstOrDefault(x => x.id == id);
            string? name = Name(data);
            if (s == null) return NotFound();
            if (name == null) return BadRequest(new { message = "Please enter a name." });
            s.Name = name;
            _context.SaveChanges();
            return Ok();
        }

        [HttpDelete("items/{id:int}")]
        public ActionResult DeleteItem(int id)
        {
            VEH_Items? s = _context.VEH_Items.FirstOrDefault(x => x.id == id);
            if (s == null) return NotFound();
            s.deleted = "Y";
            _context.SaveChanges();
            return Ok();
        }

        // ---- Vehicles ----

        [HttpPost("opened")]
        public ActionResult Opened()
        {
            ProgramLog.Write(HttpContext, _context, ProgramLog.Vehicles, "Open Vehicles Edit Screen");
            return Ok();
        }

        [HttpGet("vehicles")]
        public ActionResult<dynamic> Vehicles() =>
            Ok(_context.VEH_Vehicles.AsNoTracking().Where(v => v.deleted == "N").OrderBy(v => v.id).Select(v => new { v.id, v.Name, v.NumberPlate }).ToList());

        [HttpPost("vehicles")]
        public ActionResult<dynamic> AddVehicle([FromBody] JObject data)
        {
            string? name = Name(data);
            if (name == null) return BadRequest(new { message = "Please enter a name." });
            VEH_Vehicles v = new() { Name = name, NumberPlate = (string?)data["NumberPlate"], deleted = "N" };
            _context.VEH_Vehicles.Add(v);
            _context.SaveChanges();
            return Ok(new { v.id });
        }

        [HttpPut("vehicles/{id:int}")]
        public ActionResult UpdateVehicle(int id, [FromBody] JObject data)
        {
            VEH_Vehicles? v = _context.VEH_Vehicles.FirstOrDefault(x => x.id == id);
            string? name = Name(data);
            if (v == null) return NotFound();
            if (name == null) return BadRequest(new { message = "Please enter a name." });
            v.Name = name;
            v.NumberPlate = (string?)data["NumberPlate"];
            _context.SaveChanges();
            return Ok();
        }

        [HttpDelete("vehicles/{id:int}")]
        public ActionResult DeleteVehicle(int id)
        {
            VEH_Vehicles? v = _context.VEH_Vehicles.FirstOrDefault(x => x.id == id);
            if (v == null) return NotFound();
            v.deleted = "Y";
            _context.SaveChanges();
            return Ok();
        }

        // GET api/v1/vehiclesetup/vehicles/5 - the vehicle's checklist: sections in order, each with its items
        [HttpGet("vehicles/{id:int}")]
        public ActionResult<dynamic> Vehicle(int id)
        {
            VEH_Vehicles? v = _context.VEH_Vehicles.AsNoTracking().FirstOrDefault(x => x.id == id);
            if (v == null) return NotFound();

            SqlParameter[] p = { new("@id", id) };
            var sections = SqlQuery.Select(_context, @"select vs.id, vs.sectionid, s.Name
                from VEH_Veh_Sec vs inner join VEH_Sections s on vs.sectionid = s.id
                where vs.vehicleid = @id and vs.deleted = 'N'
                order by vs.orderby, vs.id", p);
            var items = SqlQuery.Select(_context, @"select si.id, si.sectionid, si.itemid, i.Name, si.weekly, si.monthly
                from VEH_Sec_Itm si inner join VEH_Items i on si.itemid = i.id
                where si.vehicleid = @id and si.deleted = 'N'
                order by si.orderby, si.id", p);

            return Ok(new
            {
                vehicle = new { v.id, v.Name, v.NumberPlate },
                sections = sections.Select(s => new
                {
                    id = s["id"],
                    sectionid = s["sectionid"],
                    Name = s["Name"],
                    items = items.Where(i => Equals(i["sectionid"], s["sectionid"])).ToList()
                })
            });
        }

        // POST api/v1/vehiclesetup/vehicles/5/sections { sectionid } - appended at the end
        [HttpPost("vehicles/{id:int}/sections")]
        public ActionResult AddVehicleSection(int id, [FromBody] JObject data)
        {
            int sectionId = (int?)data["sectionid"] ?? 0;
            if (!_context.VEH_Sections.Any(s => s.id == sectionId)) return BadRequest(new { message = "Unknown section." });
            if (_context.VEH_Veh_Sec.Any(s => s.vehicleid == id && s.sectionid == sectionId && s.deleted == "N"))
                return BadRequest(new { message = "This vehicle already has that section." });

            int next = (_context.VEH_Veh_Sec.Where(s => s.vehicleid == id).Max(s => s.orderby) ?? 0) + 1;
            _context.VEH_Veh_Sec.Add(new VEH_Veh_Sec { vehicleid = id, sectionid = sectionId, orderby = next, deleted = "N" });
            _context.SaveChanges();
            return Ok();
        }

        // DELETE api/v1/vehiclesetup/vehiclesections/12
        [HttpDelete("vehiclesections/{vsId:int}")]
        public ActionResult DeleteVehicleSection(int vsId)
        {
            VEH_Veh_Sec? vs = _context.VEH_Veh_Sec.FirstOrDefault(x => x.id == vsId);
            if (vs == null) return NotFound();
            vs.deleted = "Y";
            _context.SaveChanges();
            return Ok();
        }

        // POST api/v1/vehiclesetup/vehiclesections/12/move { direction: up|down }
        [HttpPost("vehiclesections/{vsId:int}/move")]
        public ActionResult MoveVehicleSection(int vsId, [FromBody] JObject data)
        {
            VEH_Veh_Sec? vs = _context.VEH_Veh_Sec.FirstOrDefault(x => x.id == vsId);
            if (vs == null) return NotFound();

            var siblings = _context.VEH_Veh_Sec.Where(x => x.vehicleid == vs.vehicleid && x.deleted == "N").OrderBy(x => x.orderby).ThenBy(x => x.id).ToList();
            Move(siblings, vs, (string?)data["direction"] == "up", (x, o) => x.orderby = o);
            _context.SaveChanges();
            return Ok();
        }

        // POST api/v1/vehiclesetup/vehicles/5/items { sectionid, itemid } - weekly and monthly by default, as Laravel
        [HttpPost("vehicles/{id:int}/items")]
        public ActionResult AddVehicleItem(int id, [FromBody] JObject data)
        {
            int sectionId = (int?)data["sectionid"] ?? 0;
            int itemId = (int?)data["itemid"] ?? 0;
            if (!_context.VEH_Items.Any(i => i.id == itemId)) return BadRequest(new { message = "Unknown item." });

            // Laravel took the max over the section across every vehicle; order within this vehicle's section
            int next = (_context.VEH_Sec_Itm.Where(s => s.vehicleid == id && s.sectionid == sectionId).Max(s => s.orderby) ?? 0) + 1;
            _context.VEH_Sec_Itm.Add(new VEH_Sec_Itm { vehicleid = id, sectionid = sectionId, itemid = itemId, orderby = next, weekly = "Y", monthly = "Y", deleted = "N" });
            _context.SaveChanges();
            return Ok();
        }

        // PUT api/v1/vehiclesetup/vehicleitems/30 { weekly?, monthly? }
        [HttpPut("vehicleitems/{itemRowId:int}")]
        public ActionResult UpdateVehicleItem(int itemRowId, [FromBody] JObject data)
        {
            VEH_Sec_Itm? si = _context.VEH_Sec_Itm.FirstOrDefault(x => x.id == itemRowId);
            if (si == null) return NotFound();
            if (data.ContainsKey("weekly")) si.weekly = (string?)data["weekly"] == "Y" ? "Y" : "N";
            if (data.ContainsKey("monthly")) si.monthly = (string?)data["monthly"] == "Y" ? "Y" : "N";
            _context.SaveChanges();
            return Ok();
        }

        [HttpDelete("vehicleitems/{itemRowId:int}")]
        public ActionResult DeleteVehicleItem(int itemRowId)
        {
            VEH_Sec_Itm? si = _context.VEH_Sec_Itm.FirstOrDefault(x => x.id == itemRowId);
            if (si == null) return NotFound();
            si.deleted = "Y";
            _context.SaveChanges();
            return Ok();
        }

        // POST api/v1/vehiclesetup/vehicleitems/30/move { direction } - within the same vehicle and section
        [HttpPost("vehicleitems/{itemRowId:int}/move")]
        public ActionResult MoveVehicleItem(int itemRowId, [FromBody] JObject data)
        {
            VEH_Sec_Itm? si = _context.VEH_Sec_Itm.FirstOrDefault(x => x.id == itemRowId);
            if (si == null) return NotFound();

            var siblings = _context.VEH_Sec_Itm.Where(x => x.vehicleid == si.vehicleid && x.sectionid == si.sectionid && x.deleted == "N")
                .OrderBy(x => x.orderby).ThenBy(x => x.id).ToList();
            Move(siblings, si, (string?)data["direction"] == "up", (x, o) => x.orderby = o);
            _context.SaveChanges();
            return Ok();
        }

        // Swap with the neighbour, then renumber 1..n so duplicate/null orderby values can't stall a move
        private static void Move<T>(List<T> ordered, T row, bool up, Action<T, int> setOrder)
        {
            int index = ordered.IndexOf(row);
            int target = up ? index - 1 : index + 1;
            if (index < 0 || target < 0 || target >= ordered.Count) return;
            (ordered[index], ordered[target]) = (ordered[target], ordered[index]);
            for (int i = 0; i < ordered.Count; i++) setOrder(ordered[i], i + 1);
        }

        private static string? Name(JObject data)
        {
            string? name = ((string?)data["Name"])?.Trim();
            return string.IsNullOrEmpty(name) ? null : name;
        }
    }
}
