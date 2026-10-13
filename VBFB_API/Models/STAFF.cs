using System;
using System.Collections.Generic;

namespace VBFB_API.Models;

public partial class STAFF
{
    public int STAFFNO { get; set; }

    public string? NAME { get; set; }

    public string? JOBTITLE { get; set; }

    public string? EXTENSION { get; set; }

    public string? PHONE { get; set; }

    public string? HOMEPHONE { get; set; }

    public string? ISACTIVE { get; set; }

    public string? APP_PASSWORD { get; set; }

    public int? MENU_NO { get; set; }

    public double? AUTH_AMT { get; set; }

    public double? STOCK_AUTH_AMT { get; set; }

    public double? NON_STOCK_AUTH_AMT { get; set; }

    public int? SECURITYPROFILEID { get; set; }

    public int? USERPROFILEID { get; set; }

    public string? LOGINID { get; set; }

    public DateTime? PASSWORD_CHANGED { get; set; }

    public DateTime? LAST_BAD_LOGIN { get; set; }

    public int? BAD_LOGIN_COUNT { get; set; }

    public DateTime? LAST_LOGIN { get; set; }

    public int? ACCOUNT_STATUS { get; set; }

    public string? EMAIL_ADDRESS { get; set; }

    public double? DISCOUNTRATE { get; set; }

    public string? PAYROLL_ID { get; set; }

    public string? IS_SUPERVISOR { get; set; }

    public string? NICKNAME { get; set; }

    public string? ABSENT { get; set; }

    public int? EMPLOYEE_CODE { get; set; }

    public int? SMTP_SEQNO { get; set; }

    public string? HAS_BUDGETS { get; set; }

    public int? REPORTS_TO_STAFFNO { get; set; }

    public string? FACEBOOK_ACCESS_TOKEN { get; set; }

    public string? LINKEDIN_TOKEN_KEY { get; set; }

    public string? LINKEDIN_TOKEN_SECRET { get; set; }

    public string? TWITTER_TOKEN_KEY { get; set; }

    public string? TWITTER_TOKEN_SECRET { get; set; }

    public string? API_ACCESS { get; set; }

    public string? MOBILE_ACCESS { get; set; }

    public string? BANKFEED_REFRESHTOKEN { get; set; }

    public string? SBR_REFRESHTOKEN { get; set; }

    public int? LAST_ACKNOWLEDGED_VERSION { get; set; }
}
