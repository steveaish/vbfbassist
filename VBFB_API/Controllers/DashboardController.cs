using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using VBFB_API.Classes;
using VBFB_API.Models;

namespace VBFB_API.Controllers
{
    /// <summary>
    /// Home page widgets (Laravel HomeController + the reports table): each active report row holds a
    /// SQL query returning TheLabel / Count (charts) or columns for a list. The queries are stored data,
    /// so they are run inside a transaction that is always rolled back - a report can read, never change.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/v1/dashboard
        [HttpGet]
        public ActionResult<dynamic> Get()
        {
            var reports = _context.reports.AsNoTracking().Where(r => r.active == 1).OrderBy(r => r.position).ToList();
            List<object> widgets = new();

            DbConnection conn = _context.Database.GetDbConnection();
            conn.Open();
            try
            {
                foreach (var report in reports)
                {
                    using DbTransaction tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);
                    try
                    {
                        using DbCommand cmd = conn.CreateCommand();
                        cmd.Transaction = tx;
                        cmd.CommandText = report.sql_script ?? "";
                        cmd.CommandTimeout = 15;
                        List<Dictionary<string, object?>> rows = new();
                        using (DbDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read() && rows.Count < 200)
                            {
                                Dictionary<string, object?> row = new();
                                for (int i = 0; i < reader.FieldCount; i++) row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                rows.Add(row);
                            }
                        }
                        widgets.Add(new { report.id, report.name, report.type, report.size, report.column_label, report.column_data, rows });
                    }
                    catch (Exception ex)
                    {
                        Logging.Log(Logging.LogLevel.Warning, $"Dashboard report {report.id} ({report.name}) failed: {ex.Message}");
                        widgets.Add(new { report.id, report.name, report.type, report.size, error = "This report could not be loaded." });
                    }
                    finally
                    {
                        tx.Rollback();
                    }
                }
            }
            finally
            {
                conn.Close();
            }

            return Ok(widgets);
        }
    }
}
