using System;
using System.Collections.Generic;

namespace VBFB_API.Models;

public partial class ebs_general_setup
{
    public int SEQNO { get; set; }

    public int? PROGNO { get; set; }

    public string? PROGNAME { get; set; }

    public string? LICENSECODE { get; set; }

    public DateTime? EXPIRYDATE { get; set; }

    public string? VERSION { get; set; }

    public int? NUMBERUSERS { get; set; }

    public string? PROGRAM_VERSION { get; set; }

    public DateTime? LastErrorTime { get; set; }

    public DateTime? lastupdated { get; set; }

    public DateTime? updatedat { get; set; }
}
