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

        private const int MinPasswordLength = 8;
        private const int MaxPasswordLength = 128;
        public const int MaxFailedLoginAttempts = 5;
        public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

        public int Id { get; set; }
        public string Name { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public string PasswordHash { get; set; }
        public int FailedLoginAttempts { get; set; }
        public DateTime? LockedOutUntil { get; set; }
        public DateTime? DeletedAt { get; set; }

        public bool IsDeleted
        {
            get { return DeletedAt.HasValue; }
        }

        public void Delete()
        {
            DeletedAt = DateTime.Now;
        }

        public void SetPassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < MinPasswordLength)
            {
                throw new ArgumentException("Password must be at least " + MinPasswordLength + " characters.");
            }
            if (password.Length > MaxPasswordLength)
            {
                throw new ArgumentException("Password cannot exceed " + MaxPasswordLength + " characters.");
            }
            PasswordHash = PasswordHasher.HashPassword(password);
        }

        public bool VerifyPassword(string password)
        {
            return !string.IsNullOrEmpty(PasswordHash) && PasswordHasher.VerifyPassword(password, PasswordHash);
        }

        public bool IsLockedOut()
        {
            return LockedOutUntil.HasValue && LockedOutUntil.Value > DateTime.Now;
        }

        public void RecordFailedLogin()
        {
            FailedLoginAttempts++;
            if (FailedLoginAttempts >= MaxFailedLoginAttempts)
            {
                LockedOutUntil = DateTime.Now.Add(LockoutDuration);
            }
        }

        public void RecordSuccessfulLogin()
        {
            FailedLoginAttempts = 0;
            LockedOutUntil = null;
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