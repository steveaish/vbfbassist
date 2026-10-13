using System.Net;
using System.Net.Mail;

namespace VBFB_API.Classes
{
    /// <summary>Sends mail through the SMTP server in VBFB_API.dat (SMTP2GO, as Laravel did).</summary>
    public static class Mailer
    {
        public class Attachment
        {
            public string FileName { get; set; } = "";
            public byte[] Content { get; set; } = Array.Empty<byte>();
            public string ContentType { get; set; } = "application/octet-stream";
        }

        public static void Send(IEnumerable<string> to, string subject, string body, IEnumerable<Attachment>? attachments = null, string? fromAddress = null)
        {
            Ready cfg = Globals.Ready;
            if (string.IsNullOrEmpty(cfg.SmtpHost)) throw new InvalidOperationException("SMTP is not configured in VBFB_API.dat.");

            using MailMessage message = new()
            {
                From = new MailAddress(fromAddress ?? cfg.MailFromAddress, cfg.MailFromName),
                Subject = subject,
                Body = body
            };
            foreach (string address in to.Where(a => !string.IsNullOrWhiteSpace(a))) message.To.Add(address.Trim());
            if (message.To.Count == 0) throw new InvalidOperationException("There is no email address to send to.");

            List<MemoryStream> streams = new();
            foreach (Attachment a in attachments ?? Enumerable.Empty<Attachment>())
            {
                MemoryStream ms = new(a.Content);
                streams.Add(ms);
                message.Attachments.Add(new System.Net.Mail.Attachment(ms, a.FileName, a.ContentType));
            }

            try
            {
                using SmtpClient client = new(cfg.SmtpHost, cfg.SmtpPort)
                {
                    EnableSsl = cfg.SmtpUseSsl,
                    Credentials = string.IsNullOrEmpty(cfg.SmtpUsername) ? null : new NetworkCredential(cfg.SmtpUsername, cfg.SmtpPassword)
                };
                client.Send(message);
                Logging.Log(Logging.LogLevel.Info, $"Emailed '{subject}' to {string.Join(", ", message.To.Select(t => t.Address))}");
            }
            finally
            {
                streams.ForEach(s => s.Dispose());
            }
        }
    }
}
