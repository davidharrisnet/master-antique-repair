using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterAntiqueRepair
{
    public class UserRole
    {

        public int Id { get; set; }
        public enum Role
        {
            CUSTOMER,
            CRAFTSMAN,
            MANAGER,
            OWNER
        }

       public Role UserRoleValue { get; set; }
    }
}
