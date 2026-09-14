using System;
using System.Linq;
using System.Text;

public partial class TestEF : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        using (var db = new MasterAntiqueRepair.TestDbContext())
        {
            // 1) Customers submit new Tickets
            var customers = new[]
            {
                new MasterAntiqueRepair.Customer { Name = "Alice", CreatedAt = DateTime.Now },
                new MasterAntiqueRepair.Customer { Name = "Bob", CreatedAt = DateTime.Now },
                new MasterAntiqueRepair.Customer { Name = "Carol", CreatedAt = DateTime.Now },
            };
            var descriptions = new[] { "Repair antique clock", "Restore wooden chair", "Fix porcelain vase" };

            for (int i = 0; i < customers.Length; i++)
            {
                var ticket = new MasterAntiqueRepair.Ticket { Description = descriptions[i] };
                customers[i].submit(ticket);
                db.Users.Add(customers[i]);
                db.Tickets.Add(ticket);
            }
            db.SaveChanges();

            // 2) Employees take unassigned Tickets and assign themselves
            var employees = new[]
            {
                new MasterAntiqueRepair.Employee { Name = "Dave", CreatedAt = DateTime.Now },
                new MasterAntiqueRepair.Employee { Name = "Erin", CreatedAt = DateTime.Now },
            };
            db.Users.AddRange(employees);

            var unassignedTickets = db.Tickets
                .Where(o => o.User == null)
                .OrderBy(o => o.Id)
                .Take(employees.Length)
                .ToList();

            for (int i = 0; i < unassignedTickets.Count; i++)
            {
                employees[i].TakeTicket(unassignedTickets[i]);
            }
            db.SaveChanges();

            // 3) A Manager reads all Employees and their Tickets
            var manager = new MasterAntiqueRepair.Manager { Name = "Frank", CreatedAt = DateTime.Now };
            var employeesWithTickets = manager.GetEmployeesWithTickets(db);

            var summary = new StringBuilder();
            foreach (var emp in employeesWithTickets)
            {
                summary.Append(emp.Name + ": ");
                summary.Append(string.Join(", ", emp.Tickets.Select(o => o.Description + " (" + o.State + ")")));
                summary.Append("; ");
            }

            ResultLabel.Text = "Tickets: " + db.Tickets.Count();
            UserLabel.Text = "Users: " + db.Users.Count();
            UserName.Text = summary.ToString();
        }
    }
}
