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
        }
    }

}
