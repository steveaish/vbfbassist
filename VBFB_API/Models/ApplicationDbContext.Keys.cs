using Microsoft.EntityFrameworkCore;

namespace VBFB_API.Models;

/// <summary>
/// Hand-written; survives re-scaffolding. The Laravel database has almost no primary keys, so the
/// scaffolder maps the tables as keyless (read-only) entities. EF needs a key to insert/update, so
/// each table's identity column is declared as its key here - in the model only, the database is
/// left unchanged so Laravel is unaffected.
/// </summary>
public partial class ApplicationDbContext
{
    // table -> identity column (from sys.identity_columns)
    private static readonly Dictionary<Type, string> IdentityKeys = new()
    {
        [typeof(BFR_Activities)] = "id",
        [typeof(BFR_ActivityTypes)] = "id",
        [typeof(BFR_Areas)] = "id",
        [typeof(BFR_Streets)] = "id",
        [typeof(BFR_StreetView)] = "id",
        [typeof(cadence_last)] = "seqno",
        [typeof(cadence_log)] = "seqno",
        [typeof(cadet_attendance)] = "id",
        [typeof(Cadet_Crews)] = "seqno",
        [typeof(calendar)] = "id",
        [typeof(config)] = "seqno",
        [typeof(Crews)] = "seqno",
        [typeof(CustomerMap)] = "seqno",
        [typeof(DocumentHistory)] = "seqno",
        [typeof(DocumentMaster)] = "seqno",
        [typeof(documents)] = "id",
        [typeof(ebs_general_setup)] = "SEQNO",
        [typeof(ppe_history)] = "id",
        [typeof(ppe_items)] = "id",
        [typeof(ppe_stock)] = "id",
        [typeof(ppe_Style)] = "id",
        [typeof(profiles)] = "id",
        [typeof(program_log)] = "id",
        [typeof(reports)] = "id",
        [typeof(sms_list)] = "seqno",
        [typeof(sms_message)] = "seqno",
        [typeof(STAFF)] = "STAFFNO",
        [typeof(training)] = "id",
        [typeof(usermodules)] = "seqno",
        [typeof(users)] = "id",
        [typeof(VEH_Check_Header)] = "id",
        [typeof(VEH_Items)] = "id",
        [typeof(VEH_Resolutions)] = "id",
        [typeof(VEH_Sec_Itm)] = "id",
        [typeof(VEH_Sections)] = "id",
        [typeof(VEH_Veh_Sec)] = "id",
        [typeof(VEH_Vehicles)] = "id",
    };

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        foreach ((Type type, string column) in IdentityKeys)
        {
            var entity = modelBuilder.Entity(type);
            entity.HasKey(column);
            entity.Property(column).ValueGeneratedOnAdd();
        }

        // Tables with triggers can't use EF's default OUTPUT clause on insert/update
        modelBuilder.Entity<profiles>().ToTable(tb => tb.HasTrigger("Profile_Types"));
        modelBuilder.Entity<VEH_Check_Header>().ToTable(tb => tb.HasTrigger("VEH_Check_Header_null"));
    }
}
