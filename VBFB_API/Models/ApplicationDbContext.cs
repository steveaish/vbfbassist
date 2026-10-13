using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace VBFB_API.Models;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BFR_Activities> BFR_Activities { get; set; }

    public virtual DbSet<BFR_ActivityTypes> BFR_ActivityTypes { get; set; }

    public virtual DbSet<BFR_Areas> BFR_Areas { get; set; }

    public virtual DbSet<BFR_StreetView> BFR_StreetView { get; set; }

    public virtual DbSet<BFR_Streets> BFR_Streets { get; set; }

    public virtual DbSet<Cadet_Crews> Cadet_Crews { get; set; }

    public virtual DbSet<Crews> Crews { get; set; }

    public virtual DbSet<CustomerMap> CustomerMap { get; set; }

    public virtual DbSet<DocumentHistory> DocumentHistory { get; set; }

    public virtual DbSet<DocumentMaster> DocumentMaster { get; set; }

    public virtual DbSet<STAFF> STAFF { get; set; }

    public virtual DbSet<VEH_Check_Header> VEH_Check_Header { get; set; }

    public virtual DbSet<VEH_Check_Lines> VEH_Check_Lines { get; set; }

    public virtual DbSet<VEH_Items> VEH_Items { get; set; }

    public virtual DbSet<VEH_Resolutions> VEH_Resolutions { get; set; }

    public virtual DbSet<VEH_Sec_Itm> VEH_Sec_Itm { get; set; }

    public virtual DbSet<VEH_Sections> VEH_Sections { get; set; }

    public virtual DbSet<VEH_Veh_Sec> VEH_Veh_Sec { get; set; }

    public virtual DbSet<VEH_Vehicles> VEH_Vehicles { get; set; }

    public virtual DbSet<cadence_last> cadence_last { get; set; }

    public virtual DbSet<cadence_log> cadence_log { get; set; }

    public virtual DbSet<cadet_attendance> cadet_attendance { get; set; }

    public virtual DbSet<calendar> calendar { get; set; }

    public virtual DbSet<config> config { get; set; }

    public virtual DbSet<documents> documents { get; set; }

    public virtual DbSet<ebs_general_setup> ebs_general_setup { get; set; }

    public virtual DbSet<ppe_Style> ppe_Style { get; set; }

    public virtual DbSet<ppe_history> ppe_history { get; set; }

    public virtual DbSet<ppe_items> ppe_items { get; set; }

    public virtual DbSet<ppe_stock> ppe_stock { get; set; }

    public virtual DbSet<profiles> profiles { get; set; }

    public virtual DbSet<program_log> program_log { get; set; }

    public virtual DbSet<reports> reports { get; set; }

    public virtual DbSet<sms_list> sms_list { get; set; }

    public virtual DbSet<sms_message> sms_message { get; set; }

    public virtual DbSet<training> training { get; set; }

    public virtual DbSet<usermodules> usermodules { get; set; }

    public virtual DbSet<users> users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BFR_Activities>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.activitydate).HasColumnType("datetime");
            entity.Property(e => e.duration).HasDefaultValue(0.0);
            entity.Property(e => e.id).ValueGeneratedOnAdd();
            entity.Property(e => e.notes)
                .HasMaxLength(1024)
                .IsUnicode(false);
            entity.Property(e => e.starttime)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<BFR_ActivityTypes>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.ActivityDesc)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<BFR_Areas>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.AreaName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<BFR_StreetView>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<BFR_Streets>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.StreetName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Cadet_Crews>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.CrewName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.seqno).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Crews>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.CrewName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.deleted)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.seqno).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<CustomerMap>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.CustomerName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DatabaseName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.seqno).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<DocumentHistory>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.createddate).HasColumnType("datetime");
            entity.Property(e => e.description)
                .HasMaxLength(4096)
                .IsUnicode(false);
            entity.Property(e => e.documentname)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.filename)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.lastupdated).HasColumnType("datetime");
            entity.Property(e => e.originalfilename)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.seqno).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<DocumentMaster>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.Author)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.createddate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.deleted)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.description)
                .HasMaxLength(4096)
                .IsUnicode(false);
            entity.Property(e => e.documentname)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.filename)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.lastupdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.originalfilename)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.seqno).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<STAFF>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.ABSENT)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.API_ACCESS)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.APP_PASSWORD)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.BANKFEED_REFRESHTOKEN)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.EMAIL_ADDRESS)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EXTENSION)
                .HasMaxLength(12)
                .IsUnicode(false);
            entity.Property(e => e.FACEBOOK_ACCESS_TOKEN).IsUnicode(false);
            entity.Property(e => e.HAS_BUDGETS)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.HOMEPHONE)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.ISACTIVE)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.IS_SUPERVISOR)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.JOBTITLE)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.LAST_BAD_LOGIN).HasColumnType("datetime");
            entity.Property(e => e.LAST_LOGIN).HasColumnType("datetime");
            entity.Property(e => e.LINKEDIN_TOKEN_KEY).IsUnicode(false);
            entity.Property(e => e.LINKEDIN_TOKEN_SECRET).IsUnicode(false);
            entity.Property(e => e.LOGINID)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.MOBILE_ACCESS)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.NAME)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.NICKNAME)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.PASSWORD_CHANGED).HasColumnType("datetime");
            entity.Property(e => e.PAYROLL_ID)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.PHONE)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.SBR_REFRESHTOKEN)
                .HasMaxLength(1024)
                .IsUnicode(false);
            entity.Property(e => e.STAFFNO).ValueGeneratedOnAdd();
            entity.Property(e => e.TWITTER_TOKEN_KEY).IsUnicode(false);
            entity.Property(e => e.TWITTER_TOKEN_SECRET).IsUnicode(false);
        });

        modelBuilder.Entity<VEH_Check_Header>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable(tb => tb.HasTrigger("VEH_Check_Header_null"));

            entity.Property(e => e.checkeddate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.completeddate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.createddate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.deleted)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.id).ValueGeneratedOnAdd();
            entity.Property(e => e.monthly)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.signeddate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.signedoffname)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<VEH_Check_Lines>(entity =>
        {
            entity.HasKey(e => e.id).HasName("VEH_Check_Lines_ID");

            entity.Property(e => e._checked)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Not Checked")
                .HasColumnName("checked");
            entity.Property(e => e.comments)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.remedy)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<VEH_Items>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.Name)
                .HasMaxLength(4096)
                .IsUnicode(false);
            entity.Property(e => e.deleted)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<VEH_Resolutions>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.Resolution)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<VEH_Sec_Itm>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.deleted)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.id).ValueGeneratedOnAdd();
            entity.Property(e => e.monthly)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.weekly)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
        });

        modelBuilder.Entity<VEH_Sections>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.deleted)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<VEH_Veh_Sec>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.deleted)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<VEH_Vehicles>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NumberPlate)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.deleted)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<cadence_last>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.DatabaseName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.posttime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.seqno).ValueGeneratedOnAdd();
            entity.Property(e => e.type)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<cadence_log>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.DatabaseName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.description)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.posttime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.seqno).ValueGeneratedOnAdd();
            entity.Property(e => e.type)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.updatedat)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<cadet_attendance>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.hours).HasDefaultValue(0.0);
            entity.Property(e => e.id).ValueGeneratedOnAdd();
            entity.Property(e => e.lastupdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.thedate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<calendar>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.colour)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.description)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.endtime).HasColumnType("datetime");
            entity.Property(e => e.id).ValueGeneratedOnAdd();
            entity.Property(e => e.longdesc)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.repeatguid)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.thedate).HasColumnType("datetime");
            entity.Property(e => e.thetime).HasColumnType("datetime");
        });

        modelBuilder.Entity<config>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.FieldName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FieldValue)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.LongDescription)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.configgroup)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.defaultvalue)
                .HasMaxLength(1024)
                .IsUnicode(false);
            entity.Property(e => e.lastupdated).HasColumnType("datetime");
            entity.Property(e => e.seqno).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<documents>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.created_at)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.description)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.doctype)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.filename)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.id).ValueGeneratedOnAdd();
            entity.Property(e => e.origfilename)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ebs_general_setup>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.EXPIRYDATE).HasColumnType("datetime");
            entity.Property(e => e.LICENSECODE)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LastErrorTime).HasColumnType("datetime");
            entity.Property(e => e.PROGNAME)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PROGRAM_VERSION)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.SEQNO).ValueGeneratedOnAdd();
            entity.Property(e => e.VERSION)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.lastupdated).HasColumnType("datetime");
            entity.Property(e => e.updatedat).HasColumnType("datetime");
        });

        modelBuilder.Entity<ppe_Style>(entity =>
        {
            entity.HasNoKey();

            entity.HasIndex(e => e.id, "ppe_style_id");

            entity.HasIndex(e => e.ItemSeqno, "ppe_style_itemseqno");

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.deleted)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.id).ValueGeneratedOnAdd();
            entity.Property(e => e.quantity).HasDefaultValue(0.0);
        });

        modelBuilder.Entity<ppe_history>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.Quantity).HasDefaultValue(0.0);
            entity.Property(e => e.historydate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.id).ValueGeneratedOnAdd();
            entity.Property(e => e.notes)
                .HasMaxLength(1000)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ppe_items>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.deleted)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<ppe_stock>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.acknowledged)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.deleted)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.id).ValueGeneratedOnAdd();
            entity.Property(e => e.issuedate).HasColumnType("datetime");
            entity.Property(e => e.laststocktakedate).HasColumnType("datetime");
            entity.Property(e => e.notes)
                .HasMaxLength(2000)
                .IsUnicode(false);
            entity.Property(e => e.notified)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.returndate).HasColumnType("datetime");
            entity.Property(e => e.returned)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
        });

        modelBuilder.Entity<profiles>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable(tb => tb.HasTrigger("Profile_Types"));

            entity.Property(e => e.BFRResident)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.IsVehicleMaint)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.ViewMembers)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.address)
                .HasMaxLength(4096)
                .IsUnicode(false);
            entity.Property(e => e.address1)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.address2)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.address3)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.address4)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.allergies)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.allergydetails)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.alltypes)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.auxmember)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.bartactive)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.cadet)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.cadetleader)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.cananyoneelsepickup)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.canbulksms)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.caneditcalendar)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.caneditdocuments)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.canseecalendar)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.captain)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.caregiverhome)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.caregivermobile)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.caregivername)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.caregiverwork)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.comments)
                .HasMaxLength(4095)
                .IsUnicode(false);
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.dfesnumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.dlicense)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.dob)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.dob_date).HasColumnType("datetime");
            entity.Property(e => e.email)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.firstlinefirefighter)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.firstname)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.homephone)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.id).ValueGeneratedOnAdd();
            entity.Property(e => e.isactive)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.isadmin)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.iscoordinator)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.isfacilitator)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.isstreetcoordinator)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.lastname)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.lieutenant)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.lifemember)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.medicaldetails)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.medicalyn)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.mobilephone)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.otherofficer)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.password)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.phone)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.photossocialmedia)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.prefcomm)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.president)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.secondlinefirefighter)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.sex)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.signoffcode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.startdate)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.startdate_date).HasColumnType("datetime");
            entity.Property(e => e.updated_at)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.username)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.whocannotpickup)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.whocanpickup)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.workphone)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.year10)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N");
            entity.Property(e => e.year15)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N");
            entity.Property(e => e.year20)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N");
            entity.Property(e => e.year25)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N");
            entity.Property(e => e.year30)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N");
            entity.Property(e => e.year5)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N");
        });

        modelBuilder.Entity<program_log>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.description)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.id).ValueGeneratedOnAdd();
            entity.Property(e => e.posttime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<reports>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.column_data).HasMaxLength(255);
            entity.Property(e => e.column_label).HasMaxLength(255);
            entity.Property(e => e.created_at).HasColumnType("datetime");
            entity.Property(e => e.html_link).HasMaxLength(255);
            entity.Property(e => e.icon).HasMaxLength(255);
            entity.Property(e => e.id).ValueGeneratedOnAdd();
            entity.Property(e => e.name).HasMaxLength(255);
            entity.Property(e => e.queryname)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.sql_script)
                .HasMaxLength(4096)
                .IsUnicode(false);
            entity.Property(e => e.type).HasMaxLength(255);
            entity.Property(e => e.updated_at).HasColumnType("datetime");
        });

        modelBuilder.Entity<sms_list>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.posttime).HasColumnType("datetime");
            entity.Property(e => e.senttime).HasColumnType("datetime");
            entity.Property(e => e.seqno).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<sms_message>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.createddate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.message)
                .HasMaxLength(1024)
                .IsUnicode(false);
            entity.Property(e => e.sent)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.senttime).HasColumnType("datetime");
            entity.Property(e => e.seqno).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<training>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.Description)
                .HasMaxLength(4096)
                .IsUnicode(false);
            entity.Property(e => e.IsActive)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("Y")
                .IsFixedLength();
            entity.Property(e => e.id).ValueGeneratedOnAdd();
            entity.Property(e => e.lastupdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.name)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.resources)
                .HasMaxLength(4096)
                .IsUnicode(false);
        });

        modelBuilder.Entity<usermodules>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.IsActive)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.module)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.seqno).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<users>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.created_at).HasColumnType("datetime");
            entity.Property(e => e.email).HasMaxLength(255);
            entity.Property(e => e.id).ValueGeneratedOnAdd();
            entity.Property(e => e.name).HasMaxLength(255);
            entity.Property(e => e.password).HasMaxLength(255);
            entity.Property(e => e.password2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.profile_cadets)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.profile_profiles)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.remember_token).HasMaxLength(100);
            entity.Property(e => e.updated_at).HasColumnType("datetime");
            entity.Property(e => e.userkey).HasMaxLength(50);
            entity.Property(e => e.userkeyexpiry).HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
