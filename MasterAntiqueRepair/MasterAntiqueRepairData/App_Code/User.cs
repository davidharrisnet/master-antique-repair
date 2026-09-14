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
        public System.DateTime CreatedAt { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
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

        public Comment AddComment(Ticket ticket, string text)
        {
            if (ticket.State != State.RepairState.COMPLETED)
            {
                throw new InvalidOperationException("Comments can only be added to completed tickets.");
            }
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Comment text is required.", nameof(text));
            }

            var comment = new Comment
            {
                User = this,
                Ticket = ticket,
                Text = text.Trim(),
                CreatedAt = DateTime.Now
            };
            ticket.Comments.Add(comment);
            return comment;
        }

        public void EditComment(Comment comment, string newText)
        {
            if (comment.UserId != Id)
            {
                throw new InvalidOperationException("You can only edit your own comments.");
            }
            if (string.IsNullOrWhiteSpace(newText))
            {
                throw new ArgumentException("Comment text is required.", nameof(newText));
            }

            comment.Text = newText.Trim();
        }

        public void DeleteComment(Comment comment)
        {
            if (comment.UserId != Id)
            {
                throw new InvalidOperationException("You can only delete your own comments.");
            }
        }
    }
        
}