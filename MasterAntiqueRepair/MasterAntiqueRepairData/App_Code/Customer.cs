using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterAntiqueRepair
{
    public class Customer : MasterAntiqueRepair.User
    {
        public void submit(Order order)
        {
            order.State = State.RepairState.SUBMITTED;            
        }
    }
}
