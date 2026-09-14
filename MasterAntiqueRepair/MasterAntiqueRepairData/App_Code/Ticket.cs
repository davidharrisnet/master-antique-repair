using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterAntiqueRepair
{
    public class Ticket
    {
        private const int MaxDescriptionLength = 2000;

        public int Id { get; set; }
        public User User { get; set; }
        public Customer Customer { get; set; }
        public State.RepairState State { get; set; }

        [MaxLength(MaxDescriptionLength)]
        public String Description { get; set; }

        public DateTime? SubmittedDate { get; set; }
        public DateTime? AssignedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public static Ticket CreateSubmitted(string description, Customer customer)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException("Description is required.", nameof(description));
            }

            var trimmed = description.Trim();

            if (trimmed.Length > MaxDescriptionLength)
            {
                throw new ArgumentException("Description cannot exceed " + MaxDescriptionLength + " characters.", nameof(description));
            }

            if (trimmed.Any(c => char.IsControl(c) && c != '\n' && c != '\r' && c != '\t'))
            {
                throw new ArgumentException("Description contains invalid characters.", nameof(description));
            }

            return new Ticket
            {
                Description = trimmed,
                Customer = customer,
                State = MasterAntiqueRepair.State.RepairState.SUBMITTED,
                SubmittedDate = DateTime.Now
            };
        }
    }
}
