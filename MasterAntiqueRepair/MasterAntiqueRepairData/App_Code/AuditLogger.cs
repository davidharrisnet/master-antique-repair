using System;

namespace MasterAntiqueRepair
{
    public static class AuditLogger
    {
        public static void Log(RepairShopContext db, User actor, AuditLog.ActionType action, AuditLog.EntityKind entityType, int entityId)
        {
            db.AuditLogs.Add(new AuditLog
            {
                Timestamp = DateTime.Now,
                User = actor,
                Action = action,
                EntityType = entityType,
                EntityId = entityId
            });
        }
    }
}
