using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace FASTER.Models
{
    public class Encryption
    {
        private const string CurrentPrefix = "dpapi:";
        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("FASTER.SteamCredentials.v2");

        private static Encryption _instance;
        private Aes _legacyCrypt;

        public static Encryption Instance => _instance ??= new Encryption();

        private Encryption()
        { }

        /// <summary>True if the stored value was written by the current (DPAPI) format.</summary>
        public bool IsCurrentFormat(string stored)
        { return !string.IsNullOrEmpty(stored) && stored.StartsWith(CurrentPrefix, StringComparison.Ordinal); }

        public string EncryptData(string plaintext)
        {
            if (string.IsNullOrEmpty(plaintext))
                return string.Empty;

            try
            {
                var bytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(plaintext), Entropy, DataProtectionScope.CurrentUser);
                return CurrentPrefix + Convert.ToBase64String(bytes);
            }
            catch
            { return null; }
        }

        public string DecryptData(string encryptedtext)
        {
            if (string.IsNullOrEmpty(encryptedtext))
                return null;

            // Old values (pre-DPAPI) have no prefix. Still readable so they can be migrated.
            if (!IsCurrentFormat(encryptedtext))
                return DecryptLegacy(encryptedtext);

            try
            {
                var bytes = ProtectedData.Unprotect(Convert.FromBase64String(encryptedtext[CurrentPrefix.Length..]), Entropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            { return null; }
        }

        /// <summary>
        /// Converts an old-format value to the DPAPI format.
        /// Returns the input unchanged if it is already current, empty, or unreadable.
        /// </summary>
        public string Migrate(string stored)
        {
            if (string.IsNullOrEmpty(stored) || IsCurrentFormat(stored))
                return stored;

            var plain = DecryptLegacy(stored);
            return plain == null ? stored : EncryptData(plain) ?? stored;
        }

        #region Legacy (read-only, for migration)

        private static byte[] TruncateHash(string key, int length)
        {
            byte[] hash   = SHA1.HashData(Encoding.Unicode.GetBytes(key));
            byte[] result = new byte[length];
            Array.Copy(hash, result, Math.Min(length, hash.Length));
            return result;
        }

        // Built lazily so the PowerShell serial lookup only happens when an old value needs reading.
        private Aes LegacyCrypt()
        {
            if (_legacyCrypt != null)
                return _legacyCrypt;

            var aes = Aes.Create();
            string key = Environment.UserName + SystemSerialNumber();
            aes.Key = TruncateHash(key, aes.KeySize / 8);
            aes.IV  = TruncateHash("", aes.BlockSize / 8);
            return _legacyCrypt = aes;
        }

        private string DecryptLegacy(string encryptedtext)
        {
            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedtext);

                using var ms = new System.IO.MemoryStream();
                using (var decStream = new CryptoStream(ms, LegacyCrypt().CreateDecryptor(), CryptoStreamMode.Write))
                {
                    decStream.Write(encryptedBytes, 0, encryptedBytes.Length);
                    decStream.FlushFinalBlock();
                }
                return Encoding.Unicode.GetString(ms.ToArray());
            }
            catch
            { return null; }
        }

        private static string SystemSerialNumber()
        {
            try
            {
                var powershell = Process.Start(new ProcessStartInfo
                {
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = "powershell",
                    Arguments = "Get-WmiObject Win32_BaseBoard | select SerialNumber"
                });
                powershell?.WaitForExit();
                var output = powershell?.StandardOutput.ReadToEnd();
                return output?.Replace("\r", "").Split('\n')[3];
            }
            catch (Exception) { return "EXCEPTION_ON_QUERY"; }
        }

        #endregion
    }
}