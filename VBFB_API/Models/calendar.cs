using System;
using System.Collections.Generic;

namespace VBFB_API.Models;

public partial class calendar
{
    public int id { get; set; }

    public DateTime? thedate { get; set; }

    public DateTime? thetime { get; set; }

    public string? description { get; set; }

    public string? longdesc { get; set; }

    public DateTime? endtime { get; set; }

    public string? colour { get; set; }

    public string? repeatguid { get; set; }
}
