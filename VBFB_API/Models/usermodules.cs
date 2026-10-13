using System;
using System.Collections.Generic;

namespace VBFB_API.Models;

public partial class usermodules
{
    public int seqno { get; set; }

    public int? userid { get; set; }

    public string? module { get; set; }

    public string? IsActive { get; set; }
}
