using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace MasterAntiqueRepair
{
    public class Manager : User
    {
        public List<Employee> GetEmployeesWithOrders(DbContext db)
        {
            return db.Set<Employee>().ToList();
        }
    }
}
