using System.Linq;

namespace MasterAntiqueRepair
{
    public class PasswordResetTokenRepository
    {
        private readonly RepairShopContext _db;

        public PasswordResetTokenRepository(RepairShopContext db)
        {
            _db = db;
        }

        public void Add(PasswordResetToken token)
        {
            _db.PasswordResetTokens.Add(token);
        }

        public PasswordResetToken GetByToken(string token)
        {
            return _db.PasswordResetTokens.FirstOrDefault(t => t.Token == token);
        }
    }
}
