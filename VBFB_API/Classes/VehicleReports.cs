using Microsoft.Data.SqlClient;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;
using System.Text;
using VBFB_API.Models;

namespace VBFB_API.Classes
{
    /// <summary>
    /// Vehicle check reports: the inspection report (PDF, and the CSV Laravel used to email) and the
    /// City of Armadale "Vehicle/Plant Defect Report" form for lines marked "To be reported to defect report".
    /// </summary>
    public static class VehicleReports
    {
        public const string DefectRemedy = "To be reported to defect report";

        public class CheckData
        {
            public Dictionary<string, object?> Header { get; set; } = new();
            public List<Dictionary<string, object?>> Lines { get; set; } = new();

            public string VehicleName => Header["Name"]?.ToString() ?? "";
            public string CheckType => Header["monthly"]?.ToString() == "Y" ? "Monthly" : "Weekly";
            public DateTime CreatedDate => Header["createddate"] is DateTime d ? d : DateTime.MinValue;
            public string FileStem => $"Vehicle Inspection {Safe(VehicleName)} {CreatedDate:yyyy-MM-dd}";
        }

        public static CheckData? Load(ApplicationDbContext db, int checkId)
        {
            var header = SqlQuery.Select(db, @"select h.*, v.Name, v.NumberPlate,
                    (p.firstname + ' ' + p.lastname) as CreatedByName
                from VEH_Check_Header h
                inner join VEH_Vehicles v on h.vehicleid = v.id
                left join profiles p on h.createdby = p.id
                where h.id = @id", new[] { new SqlParameter("@id", checkId) });
            if (header.Count == 0) return null;

            return new CheckData
            {
                Header = header[0],
                Lines = SqlQuery.Select(db, @"select l.id, l.checked, l.comments, l.remedy, s.Name as sectionname, i.Name as itemname
                    from VEH_Check_Lines l
                    inner join VEH_Sections s on l.sectionid = s.id
                    inner join VEH_Items i on l.itemid = i.id
                    where l.checkid = @id
                    order by l.id", new[] { new SqlParameter("@id", checkId) })
            };
        }

        public static byte[] InspectionPdf(CheckData data)
        {
            using PdfDocument doc = new();
            doc.PageSettings.Margins.All = 30;
            PdfPage page = doc.Pages.Add();
            PdfGraphics g = page.Graphics;

            PdfFont title = new PdfStandardFont(PdfFontFamily.Helvetica, 16, PdfFontStyle.Bold);
            PdfFont normal = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
            PdfFont bold = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
            PdfColor green = new(18, 101, 74);

            float y = 0;
            g.DrawString("Vehicle Inspection Report", title, new PdfSolidBrush(green), new PointF(0, y));
            y += 24;
            g.DrawString($"Vehicle: {data.VehicleName}", bold, PdfBrushes.Black, new PointF(0, y)); y += 13;
            g.DrawString($"Check type: {data.CheckType}     Check date: {data.CreatedDate:dd/MM/yyyy}", normal, PdfBrushes.Black, new PointF(0, y)); y += 13;
            g.DrawString($"Created by: {data.Header["CreatedByName"]}     Signed off by: {data.Header["signedoffname"]}", normal, PdfBrushes.Black, new PointF(0, y)); y += 20;

            PdfGrid grid = new();
            grid.Columns.Add(5);
            grid.Headers.Add(1);
            string[] heads = { "Section", "Item", "Result", "Comment", "Remedy" };
            for (int i = 0; i < heads.Length; i++) grid.Headers[0].Cells[i].Value = heads[i];
            grid.Headers[0].Style = new PdfGridRowStyle { Font = bold, BackgroundBrush = new PdfSolidBrush(green), TextBrush = PdfBrushes.White };

            string lastSection = "";
            foreach (var line in data.Lines)
            {
                PdfGridRow row = grid.Rows.Add();
                string section = line["sectionname"]?.ToString() ?? "";
                row.Cells[0].Value = section == lastSection ? "" : section;
                lastSection = section;
                row.Cells[1].Value = line["itemname"]?.ToString() ?? "";
                string result = line["checked"]?.ToString() switch { "Checked Pass" => "PASS", "Checked Fail" => "FAIL", _ => "NOT CHECKED" };
                row.Cells[2].Value = result;
                if (result != "PASS") row.Cells[2].Style.TextBrush = new PdfSolidBrush(new PdfColor(200, 17, 14));
                row.Cells[3].Value = line["comments"]?.ToString() ?? "";
                row.Cells[4].Value = line["remedy"]?.ToString() ?? "";
                row.Style.Font = normal;
            }

            float[] widths = { 95, 165, 55, 120, 100 };
            for (int i = 0; i < widths.Length; i++) grid.Columns[i].Width = widths[i];
            grid.Draw(page, new PointF(0, y));

            using MemoryStream ms = new();
            doc.Save(ms);
            return ms.ToArray();
        }

        /// <summary>The CSV Laravel emailed (kept so existing spreadsheets/processes still work).</summary>
        public static byte[] InspectionCsv(CheckData data)
        {
            StringBuilder sb = new();
            sb.Append($"Vehicle Check:,,{Csv(data.VehicleName)}\r\n\r\n");
            sb.Append($"Check Type:,,{data.CheckType}\r\n");
            sb.Append($"Check Date:,,{data.CreatedDate:dd/MM/yyyy}\r\n\r\n");
            sb.Append("Section,Item,Pass,Fail,Comment,Remedy\r\n");
            string lastSection = "";
            foreach (var line in data.Lines)
            {
                string section = line["sectionname"]?.ToString() ?? "";
                bool pass = line["checked"]?.ToString() == "Checked Pass";
                sb.Append($"{(section == lastSection ? "" : Csv(section))},{Csv(line["itemname"])},{(pass ? "PASS" : "")},{(pass ? "" : "FAIL")},{Csv(line["comments"])},{Csv(line["remedy"])}\r\n");
                lastSection = section;
            }
            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public static byte[] DefectPdf(CheckData data)
        {
            var defects = data.Lines.Where(l => l["checked"]?.ToString() != "Checked Pass" && l["remedy"]?.ToString() == DefectRemedy).ToList();

            using PdfDocument doc = new();
            doc.PageSettings.Margins.All = 40;
            PdfPage page = doc.Pages.Add();

            PdfFont small = new PdfStandardFont(PdfFontFamily.Helvetica, 8);
            PdfFont normal = new PdfStandardFont(PdfFontFamily.Helvetica, 11);
            PdfFont heading = new PdfStandardFont(PdfFontFamily.Helvetica, 18, PdfFontStyle.Bold);
            PdfPen pen = new(PdfBrushes.Black, 0.75f);

            PdfGrid grid = new();
            grid.Columns.Add(5);
            grid.Style.CellPadding = new PdfPaddings(3, 3, 3, 3);

            PdfGridRow AddRow(float height, PdfFont font, params (string text, int span)[] cells)
            {
                PdfGridRow row = grid.Rows.Add();
                row.Height = height;
                int col = 0;
                foreach (var (text, span) in cells)
                {
                    row.Cells[col].Value = text;
                    row.Cells[col].Style.Font = font;
                    if (span > 1) row.Cells[col].ColumnSpan = span;
                    col += span;
                }
                foreach (PdfGridCell c in row.Cells) c.Style.Borders.All = pen;
                return row;
            }

            PdfGridRow top = AddRow(20, normal, ("CITY OF ARMADALE", 3), ("ORIGINAL TO WORKSHOP", 2));
            top.Cells[3].StringFormat = new PdfStringFormat(PdfTextAlignment.Right);
            PdfGridRow title = AddRow(32, heading, ("VEHICLE/PLANT DEFECT REPORT", 5));
            title.Cells[0].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            AddRow(14, small, ("PLANT No.", 1), ("SPEEDO", 1), ("DRIVER", 1), ("DRIVER'S SIGNATURE", 1), ("DATE", 1));
            AddRow(30, normal, ("", 1), ("", 1), ("", 1), ("", 1), ("", 1));
            AddRow(14, small, ("LIST OF DEFECTS", 3), ("SECTION SUPERVISOR'S SIGNATURE", 2));

            // Laravel numbered every defect "1." - number them properly
            string list = defects.Count == 0
                ? "(No lines are marked \"" + DefectRemedy + "\")"
                : string.Join("\n", defects.Select((d, i) => $"{i + 1}. {d["sectionname"]} - {d["itemname"]} - {d["comments"]}"));
            AddRow(Math.Max(120, 18 * defects.Count + 20), normal, (list, 3), ("", 2));

            AddRow(70, small, ("CHIEF MECHANIC'S INSTRUCTIONS:", 4), ("DATE:", 1));
            AddRow(80, small, ("MECHANICS REMARKS AND PARTS:", 5));
            AddRow(30, normal, ("", 1), ("", 2), ("", 2));
            AddRow(14, small, ("DATE COMPLETED", 1), ("MECHANIC'S SIGNATURE", 2), ("CHIEF MECHANIC'S SIGNATURE", 2));

            grid.Draw(page, new PointF(0, 0));

            using MemoryStream ms = new();
            doc.Save(ms);
            return ms.ToArray();
        }

        private static string Csv(object? value)
        {
            string s = value?.ToString() ?? "";
            return s.Contains(',') || s.Contains('"') || s.Contains('\n') ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;
        }

        private static string Safe(string s) => string.Concat(s.Split(Path.GetInvalidFileNameChars())).Trim();
    }
}
