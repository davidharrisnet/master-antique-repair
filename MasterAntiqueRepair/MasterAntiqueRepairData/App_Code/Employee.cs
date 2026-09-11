using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterAntiqueRepair
{
    public class Employee : MasterAntiqueRepair.User
    {
        public void TakeOrder(Order order)
        {
            order.User = this;
            order.State = State.RepairState.INPROGRESS;
            order.AssignedDate = DateTime.Now;
        }

        public void CompleteOrder(Order order, string comment)
        {
            order.State = State.RepairState.COMPLETED;
            order.Comment = comment;
            order.CompletedDate = DateTime.Now;
        }
    }

}
