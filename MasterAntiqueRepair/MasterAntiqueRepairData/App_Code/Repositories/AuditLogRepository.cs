using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace MasterAntiqueRepair
{
    public class AuditLogRepository
    {
        private readonly RepairShopContext _db;

        public AuditLogRepository(RepairShopContext db)
        {
            _db = db;
        }

        public List<AuditLog> GetAll(int? entityIdFilter)
        {
            var query = _db.AuditLogs.Include(a => a.User).AsQueryable();

            if (entityIdFilter.HasValue)
            {
                query = query.Where(a => a.EntityId == entityIdFilter.Value);
            }

            return query.OrderByDescending(a => a.Timestamp).ToList();
        }
    }
}
