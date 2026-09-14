using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterAntiqueRepair
{
    public class Employee : MasterAntiqueRepair.User
    {
        public void TakeTicket(Ticket ticket)
        {
            ticket.User = this;
            ticket.State = State.RepairState.INPROGRESS;
            ticket.AssignedDate = DateTime.Now;
        }

        public void CompleteTicket(Ticket ticket, string comment)
        {
            ticket.State = State.RepairState.COMPLETED;
            ticket.CompletedDate = DateTime.Now;

            if (!string.IsNullOrWhiteSpace(comment))
            {
                AddComment(ticket, comment);
            }
        }
    }

}
