using System;
using System.Collections.Generic;

namespace VBFB_API.Models;

public partial class ppe_stock
{
    public int id { get; set; }

    public int? profileid { get; set; }

    public int? itemid { get; set; }

    public int? styleid { get; set; }

    public DateTime? issuedate { get; set; }

    public DateTime? returndate { get; set; }

    public DateTime? laststocktakedate { get; set; }

    public string? notes { get; set; }

    public string deleted { get; set; } = null!;

    public string returned { get; set; } = null!;

    public string notified { get; set; } = null!;

    public string acknowledged { get; set; } = null!;
}
