using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity.EntityFramework;

namespace MasterAntiqueRepair
{
    // Password hashing, lockout, and the UserName/Id identity columns now come from
    // IdentityUser<int,...> (ASP.NET Identity) instead of app-hand-rolled fields - see
    // AccountService/AuthService, which drive password/lockout state via UserManager
    // rather than calling methods on this class directly.
    public class User : IdentityUser<int, UserLogin, UserRole, UserClaim>
    {
        // Kept here (not just inlined in IdentityConfig) because they're meaningful
        // domain constants independent of which auth framework enforces them.
        public const int MaxFailedLoginAttempts = 5;
        public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

        public DateTime CreatedAt { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public DateTime? DeletedAt { get; set; }

        public bool IsDeleted
        {
            get { return DeletedAt.HasValue; }
        }

        public void Delete()
        {
            DeletedAt = DateTime.Now;
        }

        private const int MaxCommentLength = 2000;

        private static string ValidateCommentText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Comment text is required.", nameof(text));
            }

            var trimmed = text.Trim();

            if (trimmed.Length > MaxCommentLength)
            {
                throw new ArgumentException("Comment cannot exceed " + MaxCommentLength + " characters.", nameof(text));
            }

            if (trimmed.Any(c => char.IsControl(c) && c != '\n' && c != '\r' && c != '\t'))
            {
                throw new ArgumentException("Comment contains invalid characters.", nameof(text));
            }

            return trimmed;
        }

        public Comment AddComment(Ticket ticket, string text)
        {
            if (ticket.State != State.RepairState.COMPLETED)
            {
                throw new InvalidOperationException("Comments can only be added to completed tickets.");
            }

            var comment = new Comment
            {
                User = this,
                Ticket = ticket,
                Text = ValidateCommentText(text),
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

            comment.Text = ValidateCommentText(newText);
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
