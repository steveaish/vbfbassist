using System;
using System.Collections.Generic;

namespace VBFB_API.Models;

public partial class Crews
{
    public int seqno { get; set; }

    public string? CrewName { get; set; }

    public string deleted { get; set; } = null!;
}
