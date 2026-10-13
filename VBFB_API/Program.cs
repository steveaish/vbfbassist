using Microsoft.EntityFrameworkCore;
using Syncfusion.Licensing;
using VBFB_API.Classes;
using VBFB_API.Middleware;
using VBFB_API.Models;

namespace VBFB_API
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // LoadIni exits when VBFB_API.dat is missing, which would kill "dotnet ef" while it probes the host
            if (!EF.IsDesignTime)
            {
                Globals.Ready.LoadIni();
            }

            SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JFaF5cXGRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWH9ceHVcRGFZUkZyVkVWYEg=");

            var builder = WebApplication.CreateBuilder(args);

            // Newtonsoft so controllers can take JObject / dynamic bodies like ReadyApi.
            // Property names are sent exactly as the database columns (no camelCase), so the React
            // screens use the same field names as the tables and as Laravel did.
            builder.Services.AddControllers().AddNewtonsoftJson(o =>
            {
                o.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
            });

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(Globals.Ready.BuildDatabaseConnectionString(), sql =>
                {
                    sql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
                    sql.CommandTimeout(30);
                })
                .EnableDetailedErrors(builder.Environment.IsDevelopment());
            });

            builder.Services.AddCors(p => p.AddPolicy("corsapp", policy =>
            {
                policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().WithExposedHeaders("Content-Disposition");
            }));

            var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCors("corsapp");
            app.UseMiddleware<AuthenticationMiddleware>();
            app.MapControllers();

            Logging.Log(Logging.LogLevel.Info, "VBFB_API " + Globals.VersionNumber + " starting.");
            app.Run();
        }
    }
}
