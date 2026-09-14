using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace MasterAntiqueRepair
{
    public class PasswordResetToken
    {
        private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(1);

        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        [MaxLength(200)]
        public string Token { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? UsedAt { get; set; }

        public bool IsValid()
        {
            return !UsedAt.HasValue && ExpiresAt > DateTime.Now;
        }

        public static PasswordResetToken Create(User user)
        {
            var now = DateTime.Now;
            return new PasswordResetToken
            {
                UserId = user.Id,
                User = user,
                Token = GenerateToken(),
                CreatedAt = now,
                ExpiresAt = now.Add(TokenLifetime)
            };
        }

        private static string GenerateToken()
        {
            var bytes = new byte[32];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
            }

            // URL-safe base64 so the raw token can go straight into a query string.
            return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }
    }
}
