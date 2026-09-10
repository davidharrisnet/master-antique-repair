using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterAntiqueRepair
{
    public class State
    {
        public int Id { get; set; }
        public enum RepairState
        {
            SUBMITTED,
            INPROGRESS,
            COMPLETED
        }

        public RepairState RepairStateValue { get; set; }
    }
}
