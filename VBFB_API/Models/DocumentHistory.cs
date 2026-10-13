using System;
using System.Collections.Generic;

namespace VBFB_API.Models;

public partial class DocumentHistory
{
    public int seqno { get; set; }

    public int? originalseqno { get; set; }

    public DateTime? createddate { get; set; }

    public DateTime? lastupdated { get; set; }

    public string? documentname { get; set; }

    public int? uploadedby { get; set; }

    public int? versionnumber { get; set; }

    public string? filename { get; set; }

    public string? originalfilename { get; set; }

    public string? description { get; set; }
}
