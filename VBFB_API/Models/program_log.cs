using System;
using System.Collections.Generic;

namespace VBFB_API.Models;

public partial class program_log
{
    public int id { get; set; }

    public int? userid { get; set; }

    public int? type { get; set; }

    public string? description { get; set; }

    public DateTime posttime { get; set; }
}
