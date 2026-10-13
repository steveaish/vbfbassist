using System;
using System.Collections.Generic;

namespace VBFB_API.Models;

public partial class training
{
    public int id { get; set; }

    public string? name { get; set; }

    public string? resources { get; set; }

    public string? Description { get; set; }

    public DateTime lastupdated { get; set; }

    public int? lastupdatedby { get; set; }

    public string IsActive { get; set; } = null!;
}
