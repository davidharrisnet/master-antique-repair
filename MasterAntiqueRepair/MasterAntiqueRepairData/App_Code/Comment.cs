using System;

namespace MasterAntiqueRepair
{
    public class Comment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public int TicketId { get; set; }
        public virtual Ticket Ticket { get; set; }
        public String Text { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
