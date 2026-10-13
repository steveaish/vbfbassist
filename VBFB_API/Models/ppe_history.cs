using System;
using System.Collections.Generic;

namespace VBFB_API.Models;

public partial class ppe_history
{
    public int id { get; set; }

    public DateTime historydate { get; set; }

    public int? loggedinid { get; set; }

    public int? itemid { get; set; }

    public int? styleid { get; set; }

    public int? fromid { get; set; }

    public int? toid { get; set; }

    public double? Quantity { get; set; }

    public string? notes { get; set; }
}
