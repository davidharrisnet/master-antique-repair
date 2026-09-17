using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace MasterAntiqueRepair
{
    public class CommentRepository
    {
        private readonly RepairShopContext _db;

        public CommentRepository(RepairShopContext db)
        {
            _db = db;
        }

        public List<Comment> GetBetween(DateTime startInclusive, DateTime endExclusive)
        {
            return _db.Comments
                .Include(c => c.User)
                .Include(c => c.Ticket)
                .Where(c => c.CreatedAt >= startInclusive && c.CreatedAt < endExclusive)
                .ToList();
        }

        public List<Comment> SearchByTextContains(string text)
        {
            return _db.Comments
                .Include(c => c.User)
                .Where(c => c.Text.Contains(text))
                .ToList();
        }

        public Comment GetById(int id)
        {
            return _db.Comments.FirstOrDefault(c => c.Id == id);
        }
    }
}
