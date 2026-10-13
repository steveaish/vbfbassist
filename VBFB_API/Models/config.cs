using System;
using System.Collections.Generic;

namespace VBFB_API.Models;

public partial class config
{
    public int seqno { get; set; }

    public int? ProfileID { get; set; }

    public string? FieldName { get; set; }

    public string? FieldValue { get; set; }

    public string? configgroup { get; set; }

    public DateTime? lastupdated { get; set; }

    public int? TypeInfoID { get; set; }

    public string? defaultvalue { get; set; }

    public string? Description { get; set; }

    public string? LongDescription { get; set; }
}
