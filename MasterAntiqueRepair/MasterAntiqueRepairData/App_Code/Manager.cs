using System.Collections.Generic;
using System.Linq;

namespace MasterAntiqueRepair
{
    public class Manager : User
    {
        public List<Employee> GetEmployeesWithOrders(TestDbContext db)
        {
            return db.Users.OfType<Employee>().ToList();
        }
    }
}
