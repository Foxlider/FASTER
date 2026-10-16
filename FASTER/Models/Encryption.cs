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
            catch (Exception ex)
            {
                Logger.Log($"Encryption: could not encrypt a value: {ex.GetType().Name}: {ex.Message}");
                return null;
            }
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
            catch (Exception ex)
            {
                Logger.Log($"Encryption: could not decrypt a value: {ex.GetType().Name}: {ex.Message}");
                return null;
            }
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
            if (plain == null)
            {
                Logger.Log("Encryption: a stored value is in the old format and could not be read, leaving it unchanged.");
                return stored;
            }

            var upgraded = EncryptData(plain);
            Logger.Log(upgraded == null
                ? "Encryption: could not convert an old-format value to DPAPI, leaving it unchanged."
                : "Encryption: converted an old-format value to DPAPI.");
            return upgraded ?? stored;
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
            catch (Exception ex)
            {
                Logger.Log($"Encryption: could not read an old-format value: {ex.GetType().Name}: {ex.Message}");
                return null;
            }
        }

        private static string SystemSerialNumber()
        {
            try
            {
                using var powershell = Process.Start(new ProcessStartInfo
                {
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = "powershell",
                    Arguments = "Get-WmiObject Win32_BaseBoard | select SerialNumber"
                });

                if (powershell == null)
                    return null;

                // Read both streams while the process runs. Waiting first can deadlock if a pipe fills up.
                var outputTask = powershell.StandardOutput.ReadToEndAsync();
                var errorTask  = powershell.StandardError.ReadToEndAsync();

                if (!powershell.WaitForExit(15000))
                {
                    try { powershell.Kill(true); } catch (Exception) { /* already gone */ }
                    Logger.Log("Encryption: the board serial lookup took longer than 15 seconds and was stopped.");
                    return "EXCEPTION_ON_QUERY";
                }

                var output = outputTask.GetAwaiter().GetResult();
                var error  = errorTask.GetAwaiter().GetResult();
                if (!string.IsNullOrWhiteSpace(error))
                    Logger.Log($"Encryption: the board serial lookup wrote to stderr: {error.Trim()}");

                return output.Replace("\r", "").Split('\n')[3];
            }
            catch (Exception ex)
            {
                Logger.Log($"Encryption: could not read the board serial for the old key: {ex.GetType().Name}: {ex.Message}");
                return "EXCEPTION_ON_QUERY";
            }
        }

        #endregion
    }
}