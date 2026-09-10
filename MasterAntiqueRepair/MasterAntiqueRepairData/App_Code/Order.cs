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
        public State.RepairState State { get; set; }
        public String Description { get; set; }
    }
}
