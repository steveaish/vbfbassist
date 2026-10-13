using System;
using System.Collections.Generic;

namespace VBFB_API.Models;

public partial class DocumentMaster
{
    public int seqno { get; set; }

    public DateTime createddate { get; set; }

    public DateTime? lastupdated { get; set; }

    public string? documentname { get; set; }

    public int? uploadedby { get; set; }

    public int? versionnumber { get; set; }

    public string? filename { get; set; }

    public string? originalfilename { get; set; }

    public string? description { get; set; }

    public string deleted { get; set; } = null!;

    public string? Type { get; set; }

    public string? Author { get; set; }
}
