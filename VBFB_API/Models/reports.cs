using System;
using System.Collections.Generic;

namespace VBFB_API.Models;

public partial class reports
{
    public int id { get; set; }

    public int? company { get; set; }

    public string? type { get; set; }

    public string? name { get; set; }

    public string? icon { get; set; }

    public int? size { get; set; }

    public int? position { get; set; }

    public string? column_label { get; set; }

    public string? column_data { get; set; }

    public string? sql_script { get; set; }

    public string? html_link { get; set; }

    public int? active { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public int? dashboardid { get; set; }

    public string? queryname { get; set; }
}
