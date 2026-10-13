using System;
using System.Collections.Generic;

namespace VBFB_API.Models;

public partial class sms_list
{
    public int seqno { get; set; }

    public int? messageid { get; set; }

    public int? profileid { get; set; }

    public DateTime? posttime { get; set; }

    public int? status { get; set; }

    public DateTime? senttime { get; set; }
}
