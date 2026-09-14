using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterAntiqueRepair
{ 
    class Tickets
    {
         public List<Ticket> tickets { get; } = new List<Ticket>();
         public void Submit(Ticket ticket)
        {
            ticket.State = State.RepairState.SUBMITTED;
            this.tickets.Add(ticket);
        }
    }
}
