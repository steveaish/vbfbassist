using System.Security.Cryptography;
using System.Text;

namespace VBFB_API.Classes
{
    /// <summary>
    /// Encrypts the passwords stored in VBFB_API.dat. This keeps them out of plain sight in the
    /// file; it is not protection against someone who can read both the file and this binary.
    /// </summary>
    public static class Encryption
    {
        private static readonly byte[] Key = SHA256.HashData(Encoding.UTF8.GetBytes("VBFB_API.dat/7c1f2b9e-bushfire"));

        public static string EncryptString(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return "";

            using Aes aes = Aes.Create();
            aes.Key = Key;
            aes.GenerateIV();
            byte[] cipher = aes.EncryptCbc(Encoding.UTF8.GetBytes(plainText), aes.IV);
            return Convert.ToBase64String(aes.IV.Concat(cipher).ToArray());
        }

        public static string DecryptString(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return "";

            byte[] all = Convert.FromBase64String(cipherText);
            using Aes aes = Aes.Create();
            aes.Key = Key;
            return Encoding.UTF8.GetString(aes.DecryptCbc(all[16..], all[..16]));
        }

        /// <summary>
        /// False when the value isn't something EncryptString produced (not base64, too short, wrong
        /// padding) - typically a password typed into VBFB_API.dat in plain text.
        /// </summary>
        public static bool TryDecryptString(string cipherText, out string plainText)
        {
            plainText = "";
            try
            {
                byte[] all = Convert.FromBase64String(cipherText);
                // 16 byte IV + at least one 16 byte block
                if (all.Length < 32 || all.Length % 16 != 0) return false;
                plainText = DecryptString(cipherText);
                return true;
            }
            catch (Exception ex) when (ex is FormatException or CryptographicException)
            {
                return false;
            }
        }
    }
}
