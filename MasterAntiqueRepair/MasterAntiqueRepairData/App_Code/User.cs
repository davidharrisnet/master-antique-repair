using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for User
/// </summary>
/// 

namespace MasterAntiqueRepair
{
    public class User
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public UserRole UserRole { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public string PasswordHash { get; set; }

        public void SetPassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 6)
            {
                throw new ArgumentException("Password must be at least 6 characters.");
            }
            PasswordHash = PasswordHasher.HashPassword(password);
        }

        public bool VerifyPassword(string password)
        {
            return !string.IsNullOrEmpty(PasswordHash) && PasswordHasher.VerifyPassword(password, PasswordHash);
        }
    }
        
}