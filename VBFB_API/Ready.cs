using Newtonsoft.Json;
using VBFB_API.Classes;

namespace VBFB_API
{
    /// <summary>
    /// Runtime settings loaded from VBFB_API.dat (JSON) next to the executable, the same way ReadyApi
    /// loads ReadyApi.dat. Passwords are encrypted in place the first time the file is read.
    /// </summary>
    public class Ready
    {
        public string Enc { get; set; } = "N";
        public string? ServerName { get; set; }
        public string? DatabaseName { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? APIKEY { get; set; }

        // Hours a login's UserKey stays valid
        public int UserKeyHours { get; set; } = 12;

        // Uploaded documents and generated reports (copied from Laravel storage/app/uploads)
        public string UploadsPath { get; set; } = @"C:\VBFBData\uploads";

        // Address of the React app, used in links sent by email (password reset)
        public string AppUrl { get; set; } = "http://localhost:5183";

        // Mail (SMTP2GO) - Bulk SMS goes out as email to <mobile>@sms.clicksend.com
        public string? SmtpHost { get; set; }
        public int SmtpPort { get; set; } = 2525;
        public string? SmtpUsername { get; set; }
        public string? SmtpPassword { get; set; }
        public bool SmtpUseSsl { get; set; } = true;
        public string MailFromAddress { get; set; } = "noreply@vbfbassist.com";
        public string MailFromName { get; set; } = "VBFB Assist";
        public string SmsEmailDomain { get; set; } = "sms.clicksend.com";
        // ClickSend only accepts email-to-SMS from addresses registered on the account; Laravel sent from this one
        public string SmsFromAddress { get; set; } = "steveaish@hotmail.com";

        [JsonIgnore]
        public static string IniPath => Path.Combine(AppContext.BaseDirectory, "VBFB_API.dat");

        public void LoadIni()
        {
            string path = IniPath;

            if (!File.Exists(path))
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(new Ready(), Formatting.Indented));
                Logging.Log(Logging.LogLevel.Fatal, "VBFB_API.dat not found - an empty one was created at " + path + ". Fill it in and restart.");
                Environment.Exit(-3);
            }

            Ready? loaded = JsonConvert.DeserializeObject<Ready>(File.ReadAllText(path));
            if (loaded == null)
            {
                Logging.Log(Logging.LogLevel.Fatal, "VBFB_API.dat could not be read.");
                Environment.Exit(-3);
            }
            Globals.Ready = loaded;

            // With Enc = "Y" the passwords should be encrypted, but someone editing the file often types a
            // new password in plain text and leaves Enc alone. A value that won't decrypt is taken as plain
            // text (as ReadyApi does) and the file is rewritten with it encrypted.
            bool rewrite = Globals.Ready.Enc != "Y";
            string plainDb = ReadSecret(Globals.Ready.Password, Globals.Ready.Enc == "Y", ref rewrite);
            string plainSmtp = ReadSecret(Globals.Ready.SmtpPassword, Globals.Ready.Enc == "Y", ref rewrite);

            if (rewrite)
            {
                Globals.Ready.Password = Encryption.EncryptString(plainDb);
                Globals.Ready.SmtpPassword = Encryption.EncryptString(plainSmtp);
                Globals.Ready.Enc = "Y";
                try
                {
                    File.WriteAllText(path, JsonConvert.SerializeObject(Globals.Ready, Formatting.Indented));
                    Logging.Log(Logging.LogLevel.Info, "Encrypted the passwords in VBFB_API.dat.");
                }
                catch (Exception ex)
                {
                    // e.g. no write permission for the app pool - keep running with the values read
                    Logging.Log(Logging.LogLevel.Warning, "Could not rewrite VBFB_API.dat with encrypted passwords: " + ex.Message);
                }
            }
            Globals.Ready.Password = plainDb;
            Globals.Ready.SmtpPassword = plainSmtp;

            if (string.IsNullOrEmpty(Globals.Ready.ServerName) || string.IsNullOrEmpty(Globals.Ready.DatabaseName) ||
                string.IsNullOrEmpty(Globals.Ready.Username) || string.IsNullOrEmpty(Globals.Ready.Password) ||
                string.IsNullOrEmpty(Globals.Ready.APIKEY))
            {
                Logging.Log(Logging.LogLevel.Fatal, "VBFB_API.dat is missing ServerName, DatabaseName, Username, Password or APIKEY.");
                Environment.Exit(-3);
            }

            Logging.Log(Logging.LogLevel.Info, "Loaded VBFB_API.dat.");
        }

        // Plain-text value of a password from the file; flags a rewrite when it wasn't encrypted
        private static string ReadSecret(string? value, bool expectEncrypted, ref bool rewrite)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (!expectEncrypted) return value;

            if (Encryption.TryDecryptString(value, out string plain)) return plain;

            rewrite = true;
            return value;
        }

        public string BuildDatabaseConnectionString()
        {
            return $"Server={ServerName};Database={DatabaseName};User Id={Username};Password={Password};TrustServerCertificate=True;MultipleActiveResultSets=True";
        }
    }
}
