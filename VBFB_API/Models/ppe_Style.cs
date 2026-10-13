using System;
using System.Collections.Generic;

namespace VBFB_API.Models;

public partial class ppe_Style
{
    public int id { get; set; }

    public int? ItemSeqno { get; set; }

    public string? Name { get; set; }

    public string deleted { get; set; } = null!;

    public double? quantity { get; set; }
}
