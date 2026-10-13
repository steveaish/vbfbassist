using System;
using System.Collections.Generic;

namespace VBFB_API.Models;

public partial class VEH_Check_Lines
{
    public int id { get; set; }

    public int? checkid { get; set; }

    public int? sectionid { get; set; }

    public int? itemid { get; set; }

    public string _checked { get; set; } = null!;

    public string? comments { get; set; }

    public string? remedy { get; set; }
}
