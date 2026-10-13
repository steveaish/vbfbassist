using System;
using System.Collections.Generic;

namespace VBFB_API.Models;

public partial class VEH_Vehicles
{
    public int id { get; set; }

    public string? Name { get; set; }

    public string? NumberPlate { get; set; }

    public string deleted { get; set; } = null!;
}
