using System;
using System.Collections.Generic;

namespace VBFB_API.Models;

public partial class BFR_Activities
{
    public int id { get; set; }

    public int? activityid { get; set; }

    public int? profileid { get; set; }

    public int? streetid { get; set; }

    public DateTime? activitydate { get; set; }

    public string? starttime { get; set; }

    public double? duration { get; set; }

    public string? notes { get; set; }

    public int? enteredby { get; set; }

    public int? attendeed { get; set; }
}
