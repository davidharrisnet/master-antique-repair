using System;
using System.Security.Cryptography;

namespace MasterAntiqueRepair
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;

        // Iterations are embedded in every new hash ("iterations.salt.hash"), so this can be
        // raised again in the future without invalidating passwords hashed under a lower count.
        private const int Iterations = 100000;

        // Hashes created before the versioned format existed are a bare base64 blob of
        // salt+hash with a fixed, un-embedded iteration count. Still verified correctly via
        // VerifyLegacyPassword below - existing accounts are not broken by raising Iterations.
        private const int LegacyIterations = 10000;

        public static string HashPassword(string password)
        {
            using (var deriveBytes = new Rfc2898DeriveBytes(password, SaltSize, Iterations))
            {
                var salt = deriveBytes.Salt;
                var hash = deriveBytes.GetBytes(HashSize);
                return Iterations + "." + Convert.ToBase64String(salt) + "." + Convert.ToBase64String(hash);
            }
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            var parts = hashedPassword.Split('.');
            if (parts.Length != 3)
            {
                return VerifyLegacyPassword(password, hashedPassword);
            }

            int iterations;
            if (!int.TryParse(parts[0], out iterations))
            {
                return false;
            }

            byte[] salt;
            byte[] expectedHash;
            try
            {
                salt = Convert.FromBase64String(parts[1]);
                expectedHash = Convert.FromBase64String(parts[2]);
            }
            catch (FormatException)
            {
                return false;
            }

            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                var actualHash = deriveBytes.GetBytes(expectedHash.Length);
                return FixedTimeEquals(actualHash, expectedHash);
            }
        }

        private static bool VerifyLegacyPassword(string password, string hashedPassword)
        {
            byte[] combined;
            try
            {
                combined = Convert.FromBase64String(hashedPassword);
            }
            catch (FormatException)
            {
                return false;
            }

            if (combined.Length != SaltSize + HashSize)
            {
                return false;
            }

            var salt = new byte[SaltSize];
            Array.Copy(combined, 0, salt, 0, SaltSize);

            var expectedHash = new byte[HashSize];
            Array.Copy(combined, SaltSize, expectedHash, 0, HashSize);

            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, LegacyIterations))
            {
                var actualHash = deriveBytes.GetBytes(HashSize);
                return FixedTimeEquals(actualHash, expectedHash);
            }
        }

        // A byte-by-byte comparison that returns early on the first mismatch leaks (via timing)
        // how many leading bytes matched. This always inspects every byte regardless of outcome.
        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            var diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }
            return diff == 0;
        }
    }
}
