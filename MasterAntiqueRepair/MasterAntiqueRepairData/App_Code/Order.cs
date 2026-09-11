using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterAntiqueRepair
{
    public class Order
    {
        public int Id { get; set; }
        public User User { get; set; }
        public Customer Customer { get; set; }
        public State.RepairState State { get; set; }
        public String Description { get; set; }
        public String Comment { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public DateTime? AssignedDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        public static Order CreateSubmitted(string description, Customer customer)
        {
            return new Order
            {
                Description = description,
                Customer = customer,
                State = MasterAntiqueRepair.State.RepairState.SUBMITTED,
                SubmittedDate = DateTime.Now
            };
        }
    }
}
