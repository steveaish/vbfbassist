using System;
using System.Collections.Generic;

namespace VBFB_API.Models;

public partial class VEH_Veh_Sec
{
    public int id { get; set; }

    public int? vehicleid { get; set; }

    public int? sectionid { get; set; }

    public int? orderby { get; set; }

    public string deleted { get; set; } = null!;
}
