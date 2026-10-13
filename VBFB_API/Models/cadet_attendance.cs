using System;
using System.Collections.Generic;

namespace VBFB_API.Models;

public partial class cadet_attendance
{
    public int id { get; set; }

    public int? cadetid { get; set; }

    public int? trainingid { get; set; }

    public int? unitleader { get; set; }

    public double? hours { get; set; }

    public DateTime? thedate { get; set; }

    public DateTime lastupdated { get; set; }
}
