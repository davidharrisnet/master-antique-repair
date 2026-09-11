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

        public Dictionary<Customer, List<Order>> GetCustomersWithOrders(DbContext db)
        {
            // Customer doesn't have its own Orders collection - the inherited User.Orders
            // maps to Order.User (the assigned employee's Id), a separate relationship
            // from Order.Customer (who submitted it). Look submitted orders up directly
            // instead of relying on a navigation property that would (silently) always be
            // empty for a Customer.
            var customers = db.Set<Customer>().ToList();
            var orders = db.Set<Order>().Where(o => o.Customer != null).ToList();
            return customers.ToDictionary(c => c, c => orders.Where(o => o.Customer.Id == c.Id).ToList());
        }
    }
}
