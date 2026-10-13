using System;
using System.Collections.Generic;

namespace VBFB_API.Models;

public partial class profiles
{
    public int id { get; set; }

    public string? username { get; set; }

    public string? password { get; set; }

    public string? firstname { get; set; }

    public string? lastname { get; set; }

    public string? isactive { get; set; }

    public string? email { get; set; }

    public string? isadmin { get; set; }

    public string? isfacilitator { get; set; }

    public string? iscoordinator { get; set; }

    public string? isstreetcoordinator { get; set; }

    public int? usertype { get; set; }

    public string? phone { get; set; }

    public string? address1 { get; set; }

    public string? address2 { get; set; }

    public string? address3 { get; set; }

    public string? address4 { get; set; }

    public string? dlicense { get; set; }

    public string? dob { get; set; }

    public string? dfesnumber { get; set; }

    public string? bartactive { get; set; }

    public string? prefcomm { get; set; }

    public string firstlinefirefighter { get; set; } = null!;

    public string secondlinefirefighter { get; set; } = null!;

    public string president { get; set; } = null!;

    public string captain { get; set; } = null!;

    public string lieutenant { get; set; } = null!;

    public string otherofficer { get; set; } = null!;

    public string cadet { get; set; } = null!;

    public string lifemember { get; set; } = null!;

    public string auxmember { get; set; } = null!;

    public string? address { get; set; }

    public DateTime updated_at { get; set; }

    public int? crew { get; set; }

    public string? homephone { get; set; }

    public string? mobilephone { get; set; }

    public DateTime created_at { get; set; }

    public string? sex { get; set; }

    public string? caregivername { get; set; }

    public int? cadetcrew { get; set; }

    public string? caregivermobile { get; set; }

    public string? caregiverhome { get; set; }

    public string? caregiverwork { get; set; }

    public string? cananyoneelsepickup { get; set; }

    public string? whocanpickup { get; set; }

    public string? whocannotpickup { get; set; }

    public string? allergies { get; set; }

    public string? allergydetails { get; set; }

    public string? medicalyn { get; set; }

    public string? medicaldetails { get; set; }

    public string? photossocialmedia { get; set; }

    public string cadetleader { get; set; } = null!;

    public string IsVehicleMaint { get; set; } = null!;

    public int? spreadsheetid { get; set; }

    public string? workphone { get; set; }

    public string? startdate { get; set; }

    public string? comments { get; set; }

    public DateTime? dob_date { get; set; }

    public DateTime? startdate_date { get; set; }

    public string ViewMembers { get; set; } = null!;

    public string canbulksms { get; set; } = null!;

    public string BFRResident { get; set; } = null!;

    public int? streetid { get; set; }

    public string canseecalendar { get; set; } = null!;

    public string caneditcalendar { get; set; } = null!;

    public string? alltypes { get; set; }

    public string year5 { get; set; } = null!;

    public string year10 { get; set; } = null!;

    public string year15 { get; set; } = null!;

    public string year20 { get; set; } = null!;

    public string year25 { get; set; } = null!;

    public string year30 { get; set; } = null!;

    public string? signoffcode { get; set; }

    public string caneditdocuments { get; set; } = null!;
}
