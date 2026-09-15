using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace MasterAntiqueRepair
{
    public class Manager : User
    {
        public List<Employee> GetEmployeesWithTickets(DbContext db)
        {
            return db.Set<Employee>().Where(emp => !emp.DeletedAt.HasValue).ToList();
        }

        public Dictionary<Customer, List<Ticket>> GetCustomersWithTickets(DbContext db)
        {
            // Customer doesn't have its own Tickets collection - the inherited User.Tickets
            // maps to Ticket.User (the assigned employee's Id), a separate relationship
            // from Ticket.Customer (who submitted it). Look submitted tickets up directly
            // instead of relying on a navigation property that would (silently) always be
            // empty for a Customer.
            var customers = db.Set<Customer>().ToList();
            var tickets = db.Set<Ticket>().Where(o => o.Customer != null).ToList();
            return customers.ToDictionary(c => c, c => tickets.Where(o => o.Customer.Id == c.Id).ToList());
        }

        public List<Ticket> GetCompletedTickets(DbContext db)
        {
            return db.Set<Ticket>()
                .Where(t => t.State == State.RepairState.COMPLETED && t.CompletedDate.HasValue)
                .Include(t => t.User)
                .OrderBy(t => t.CompletedDate)
                .ToList();
        }
    }
}
