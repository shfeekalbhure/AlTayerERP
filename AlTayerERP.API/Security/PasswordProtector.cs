using System;
using System.Security.Cryptography;

namespace AlTayerERP.API.Security
{
    /// <summary>
    /// حماية كلمات المرور باستخدام PBKDF2، مع ترقية السجلات القديمة بعد أول دخول ناجح.
    /// </summary>
    public static class PasswordProtector
    {
        private const string Prefix = "PBKDF2";
        private const int Iterations = 100_000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        public static string Hash(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("كلمة المرور مطلوبة.", nameof(password));

            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            return Prefix + "$" + Iterations + "$" +
                Convert.ToBase64String(salt) + "$" +
                Convert.ToBase64String(hash);
        }

        public static bool Verify(string storedValue, string password, out bool needsUpgrade)
        {
            needsUpgrade = false;
            if (string.IsNullOrEmpty(storedValue) || string.IsNullOrEmpty(password))
                return false;

            // توافق مؤقت مع الحسابات القديمة: تُرقّى مباشرة بعد نجاح الدخول.
            if (!storedValue.StartsWith(Prefix + "$", StringComparison.Ordinal))
            {
                needsUpgrade = true;
                return CryptographicOperations.FixedTimeEquals(
                    System.Text.Encoding.UTF8.GetBytes(storedValue),
                    System.Text.Encoding.UTF8.GetBytes(password));
            }

            var parts = storedValue.Split('$');
            if (parts.Length != 4 || !int.TryParse(parts[1], out var iterations))
                return false;

            try
            {
                var salt = Convert.FromBase64String(parts[2]);
                var expectedHash = Convert.FromBase64String(parts[3]);
                var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                    password,
                    salt,
                    iterations,
                    HashAlgorithmName.SHA256,
                    expectedHash.Length);

                return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}