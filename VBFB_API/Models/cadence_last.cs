using System;
using System.Collections.Generic;

namespace VBFB_API.Models;

public partial class cadence_last
{
    public int seqno { get; set; }

    public string? DatabaseName { get; set; }

    public string? type { get; set; }

    public DateTime? posttime { get; set; }

    public Guid? guid { get; set; }
}
