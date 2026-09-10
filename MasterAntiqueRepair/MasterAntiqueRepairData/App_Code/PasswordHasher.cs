using System;
using System.Security.Cryptography;

namespace MasterAntiqueRepair
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 10000;

        public static string HashPassword(string password)
        {
            using (var deriveBytes = new Rfc2898DeriveBytes(password, SaltSize, Iterations))
            {
                var salt = deriveBytes.Salt;
                var hash = deriveBytes.GetBytes(HashSize);

                var combined = new byte[SaltSize + HashSize];
                Array.Copy(salt, 0, combined, 0, SaltSize);
                Array.Copy(hash, 0, combined, SaltSize, HashSize);
                return Convert.ToBase64String(combined);
            }
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            var combined = Convert.FromBase64String(hashedPassword);
            var salt = new byte[SaltSize];
            Array.Copy(combined, 0, salt, 0, SaltSize);

            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, Iterations))
            {
                var hash = deriveBytes.GetBytes(HashSize);
                for (int i = 0; i < HashSize; i++)
                {
                    if (combined[SaltSize + i] != hash[i])
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
