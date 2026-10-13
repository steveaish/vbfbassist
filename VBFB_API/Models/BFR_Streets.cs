using System;
using System.Collections.Generic;

namespace VBFB_API.Models;

public partial class BFR_Streets
{
    public int id { get; set; }

    public int? AreaID { get; set; }

    public string? StreetName { get; set; }
}
