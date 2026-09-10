using System;
using System.Linq;
using System.Text;

public partial class TestEF : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        using (var db = new MasterAntiqueRepair.TestDbContext())
        {
            // 1) Customers submit new Orders
            var customers = new[]
            {
                new MasterAntiqueRepair.Customer { Name = "Alice", CreatedAt = DateTime.Now },
                new MasterAntiqueRepair.Customer { Name = "Bob", CreatedAt = DateTime.Now },
                new MasterAntiqueRepair.Customer { Name = "Carol", CreatedAt = DateTime.Now },
            };
            var descriptions = new[] { "Repair antique clock", "Restore wooden chair", "Fix porcelain vase" };

            for (int i = 0; i < customers.Length; i++)
            {
                var order = new MasterAntiqueRepair.Order { Description = descriptions[i] };
                customers[i].submit(order);
                db.Users.Add(customers[i]);
                db.Orders.Add(order);
            }
            db.SaveChanges();

            // 2) Employees take unassigned Orders and assign themselves
            var employees = new[]
            {
                new MasterAntiqueRepair.Employee { Name = "Dave", CreatedAt = DateTime.Now },
                new MasterAntiqueRepair.Employee { Name = "Erin", CreatedAt = DateTime.Now },
            };
            db.Users.AddRange(employees);

            var unassignedOrders = db.Orders
                .Where(o => o.User == null)
                .OrderBy(o => o.Id)
                .Take(employees.Length)
                .ToList();

            for (int i = 0; i < unassignedOrders.Count; i++)
            {
                employees[i].TakeOrder(unassignedOrders[i]);
            }
            db.SaveChanges();

            // 3) A Manager reads all Employees and their Orders
            var manager = new MasterAntiqueRepair.Manager { Name = "Frank", CreatedAt = DateTime.Now };
            var employeesWithOrders = manager.GetEmployeesWithOrders(db);

            var summary = new StringBuilder();
            foreach (var emp in employeesWithOrders)
            {
                summary.Append(emp.Name + ": ");
                summary.Append(string.Join(", ", emp.Orders.Select(o => o.Description + " (" + o.State + ")")));
                summary.Append("; ");
            }

            ResultLabel.Text = "Orders: " + db.Orders.Count();
            UserLabel.Text = "Users: " + db.Users.Count();
            UserName.Text = summary.ToString();
        }
    }
}
