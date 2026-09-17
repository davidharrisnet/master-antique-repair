using System;

namespace MasterAntiqueRepair
{
    public class CommentSearchResult
    {
        public string Type { get; set; }
        public string Text { get; set; }
        public string AuthorName { get; set; }
        public DateTime Posted { get; set; }
        public int TicketId { get; set; }
    }
}
