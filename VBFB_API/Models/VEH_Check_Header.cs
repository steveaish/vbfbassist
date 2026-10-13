using System;
using System.Collections.Generic;

namespace VBFB_API.Models;

public partial class VEH_Check_Header
{
    public int id { get; set; }

    public int? vehicleid { get; set; }

    public DateTime createddate { get; set; }

    public int? createdby { get; set; }

    public DateTime checkeddate { get; set; }

    public int? checkedby { get; set; }

    public DateTime completeddate { get; set; }

    public int? completedby { get; set; }

    public DateTime signeddate { get; set; }

    public int? signedby { get; set; }

    public string monthly { get; set; } = null!;

    public int? status { get; set; }

    public string deleted { get; set; } = null!;

    public int? signedoffid { get; set; }

    public string? signedoffname { get; set; }
}
