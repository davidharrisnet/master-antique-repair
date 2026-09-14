using System;

namespace MasterAntiqueRepair
{
    public class AuditLog
    {
        public enum ActionType
        {
            CreateUser,
            Login,
            CreateTicket,
            AssignTicket,
            CompleteTicket,
            AddComment,
            EditComment,
            DeleteComment
        }

        public enum EntityKind
        {
            User,
            Ticket,
            Comment
        }

        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public ActionType Action { get; set; }
        public EntityKind EntityType { get; set; }
        public int EntityId { get; set; }
    }
}
