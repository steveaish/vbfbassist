using Microsoft.AspNetCore.StaticFiles;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace VBFB_API.Classes
{
    /// <summary>
    /// Uploaded documents in UploadsPath (VBFB_API.dat), stored flat under a random 40 character name
    /// plus the original extension - the same scheme as Laravel's store('uploads'), so the copied files
    /// and their database filenames keep working.
    /// </summary>
    public static partial class FileStore
    {
        public const long MaxBytes = 50L * 1024 * 1024;

        // Stored names are letters/digits with an optional extension - nothing that can climb out of the folder
        [GeneratedRegex(@"^[A-Za-z0-9]+(\.[A-Za-z0-9]{0,10})?$")]
        private static partial Regex StoredNameRegex();

        private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public static string Folder
        {
            get
            {
                string folder = Globals.Ready.UploadsPath;
                Directory.CreateDirectory(folder);
                return folder;
            }
        }

        /// <summary>Saves the upload and returns the stored file name.</summary>
        public static async Task<string> SaveAsync(IFormFile file)
        {
            if (file.Length == 0) throw new InvalidOperationException("The file is empty.");
            if (file.Length > MaxBytes) throw new InvalidOperationException($"Files must be smaller than {MaxBytes / 1024 / 1024} MB.");

            string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!Regex.IsMatch(ext, @"^(\.[a-z0-9]{1,10})?$")) ext = "";

            string name = RandomNumberGenerator.GetString(Alphabet, 40) + ext;
            await using FileStream fs = new(Path.Combine(Folder, name), FileMode.CreateNew);
            await file.CopyToAsync(fs);
            return name;
        }

        /// <summary>Full path of a stored file, or null when the name is invalid or the file is missing.</summary>
        public static string? PathFor(string? storedName)
        {
            if (string.IsNullOrEmpty(storedName) || !StoredNameRegex().IsMatch(storedName)) return null;
            string path = Path.Combine(Folder, storedName);
            return File.Exists(path) ? path : null;
        }

        public static string ContentType(string fileName)
        {
            return new FileExtensionContentTypeProvider().TryGetContentType(fileName, out string? type) ? type : "application/octet-stream";
        }
    }
}
