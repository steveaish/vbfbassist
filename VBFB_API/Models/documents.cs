using System;
using System.Collections.Generic;

namespace VBFB_API.Models;

public partial class documents
{
    public int id { get; set; }

    public string? description { get; set; }

    public string? filename { get; set; }

    public DateTime? created_at { get; set; }

    public string? origfilename { get; set; }

    public string? doctype { get; set; }

    public int? sourceid { get; set; }
}
