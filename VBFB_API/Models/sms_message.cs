using System;
using System.Collections.Generic;

namespace VBFB_API.Models;

public partial class sms_message
{
    public int seqno { get; set; }

    public string? message { get; set; }

    public DateTime? senttime { get; set; }

    public DateTime createddate { get; set; }

    public string sent { get; set; } = null!;
}
