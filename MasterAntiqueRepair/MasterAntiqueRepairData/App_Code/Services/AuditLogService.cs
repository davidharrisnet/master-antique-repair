using System;
using System.Collections.Generic;

namespace MasterAntiqueRepair
{
    public class AuditLogService : IDisposable
    {
        private readonly RepairShopContext _db;
        private readonly AuditLogRepository _auditLogs;
        private readonly CommentRepository _comments;

        public AuditLogService()
        {
            _db = new RepairShopContext();
            _auditLogs = new AuditLogRepository(_db);
            _comments = new CommentRepository(_db);
        }

        public List<AuditLog> GetLogs(int? entityIdFilter)
        {
            return _auditLogs.GetAll(entityIdFilter);
        }

        // Comment audit rows don't carry a TicketId directly - resolve it so the grid's
        // "View" link can still point at the right ticket. Returns null if the comment
        // itself is gone (e.g. its ticket's history predates comments existing at all).
        public int? GetTicketIdForComment(int commentId)
        {
            var comment = _comments.GetById(commentId);
            return comment != null ? (int?)comment.TicketId : null;
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
