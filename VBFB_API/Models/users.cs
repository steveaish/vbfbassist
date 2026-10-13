using System;
using System.Collections.Generic;

namespace VBFB_API.Models;

public partial class users
{
    public int id { get; set; }

    public int? company { get; set; }

    public string? name { get; set; }

    public string? email { get; set; }

    public string? password { get; set; }

    public int? status { get; set; }

    public int? admin { get; set; }

    public string? remember_token { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public string? password2 { get; set; }

    public string? profile_profiles { get; set; }

    public string? profile_cadets { get; set; }

    public string? userkey { get; set; }

    public string? resettokenhash { get; set; }

    public DateTime? resettokenexpiry { get; set; }

    public DateTime? userkeyexpiry { get; set; }
}
