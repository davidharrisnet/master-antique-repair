using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterAntiqueRepair
{ 
    class Orders
    {
         public List<Order> orders { get; } = new List<Order>();
         public void Submit(Order order)
        {
            order.State = State.RepairState.SUBMITTED;            
            this.orders.Add(order);
        }
    }
}
