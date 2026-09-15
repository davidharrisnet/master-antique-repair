using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using MasterAntiqueRepair;

public partial class ManagerView : Page
{
    // The application-level "is this name already taken?" check (used everywhere
    // below) can still lose a race between two near-simultaneous requests - it's a
    // check-then-write, not atomic. The database's own unique index on active
    // usernames is the real backstop; this just turns the resulting DbUpdateException
    // into the same friendly message instead of an unhandled 500.
    private static bool IsDuplicateUsernameViolation(DbUpdateException ex)
    {
        var sqlEx = ex.GetBaseException() as System.Data.SqlClient.SqlException;
        return sqlEx != null && (sqlEx.Number == 2601 || sqlEx.Number == 2627);
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!RepairAuthHelper.RequireRole(Response, "Manager"))
        {
            return;
        }

        if (!IsPostBack)
        {
            using (var db = new RepairShopContext())
            {
                UnassignedGrid.DataSource = db.Tickets
                    .Include(o => o.Customer)
                    .Where(o => o.User == null)
                    .OrderBy(o => o.Id)
                    .ToList();
                UnassignedGrid.DataBind();

                BindEditEmployeeDropDown(db);
                BindViewEmployeeDropDown(db);
                BindEditCustomerDropDown(db);
            }
        }
    }

    private void BindEditEmployeeDropDown(RepairShopContext db)
    {
        var employees = db.Users.OfType<Employee>().Where(emp => !emp.DeletedAt.HasValue).OrderBy(emp => emp.Name).ToList();

        EditEmployeeDropDown.Items.Clear();
        EditEmployeeDropDown.Items.Add(new ListItem("-- Select an employee --", ""));
        foreach (var employee in employees)
        {
            EditEmployeeDropDown.Items.Add(new ListItem(employee.Name, employee.Id.ToString()));
        }
    }

    private void BindViewEmployeeDropDown(RepairShopContext db)
    {
        var employees = db.Users.OfType<Employee>().Where(emp => !emp.DeletedAt.HasValue).OrderBy(emp => emp.Name).ToList();

        ViewEmployeeDropDown.Items.Clear();
        ViewEmployeeDropDown.Items.Add(new ListItem("-- Select an employee --", ""));
        foreach (var employee in employees)
        {
            ViewEmployeeDropDown.Items.Add(new ListItem(employee.Name, employee.Id.ToString()));
        }
    }

    protected void ViewEmployeeDropDown_SelectedIndexChanged(object sender, EventArgs e)
    {
        int id;
        if (!int.TryParse(ViewEmployeeDropDown.SelectedValue, out id))
        {
            ViewEmployeePanel.Visible = false;
            return;
        }

        using (var db = new RepairShopContext())
        {
            var employee = db.Users.OfType<Employee>().FirstOrDefault(emp => emp.Id == id);
            if (employee == null)
            {
                ViewEmployeePanel.Visible = false;
                BindViewEmployeeDropDown(db);
                return;
            }

            ViewEmployeePanel.Visible = true;
            ViewEmployeeNameLiteral.Text = employee.Name;

            var tickets = employee.Tickets.OrderBy(t => t.Id).ToList();
            ViewEmployeeTicketsRepeater.DataSource = tickets;
            ViewEmployeeTicketsRepeater.DataBind();
            NoEmployeeTicketsLabel.Visible = tickets.Count == 0;
        }
    }

    private void RefreshEmployeeViews(RepairShopContext db)
    {
        BindEditEmployeeDropDown(db);
        BindViewEmployeeDropDown(db);
    }

    protected void AddEmployee_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NewEmployeeUserName.Text))
        {
            AddEmployeeErrorMessage.Text = "The user name field is required.";
            return;
        }

        if (NewEmployeePassword.Text != NewEmployeeConfirmPassword.Text)
        {
            AddEmployeeErrorMessage.Text = "The password and confirmation password do not match.";
            return;
        }

        using (var db = new RepairShopContext())
        {
            if (db.Users.Any(u => u.Name == NewEmployeeUserName.Text && !u.DeletedAt.HasValue))
            {
                AddEmployeeErrorMessage.Text = "That username is already taken.";
                return;
            }

            var employee = new Employee
            {
                Name = NewEmployeeUserName.Text,
                CreatedAt = DateTime.Now
            };

            try
            {
                employee.SetPassword(NewEmployeePassword.Text);
            }
            catch (ArgumentException ex)
            {
                AddEmployeeErrorMessage.Text = ex.Message;
                return;
            }

            db.Users.Add(employee);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException ex) when (IsDuplicateUsernameViolation(ex))
            {
                AddEmployeeErrorMessage.Text = "That username is already taken.";
                return;
            }

            var managerId = RepairAuthHelper.GetCurrentUserId();
            var manager = db.Users.OfType<Manager>().FirstOrDefault(m => m.Id == managerId);
            if (manager != null)
            {
                AuditLogger.Log(db, manager, AuditLog.ActionType.CreateUser, AuditLog.EntityKind.User, employee.Id);
                db.SaveChanges();
            }

            AddEmployeeErrorMessage.Text = string.Empty;
            AddEmployeeSuccessMessage.Text = "Employee account created for " + employee.Name + ".";
            NewEmployeeUserName.Text = string.Empty;
            NewEmployeePassword.Text = string.Empty;
            NewEmployeeConfirmPassword.Text = string.Empty;

            RefreshEmployeeViews(db);
        }
    }

    protected void EditEmployeeDropDown_SelectedIndexChanged(object sender, EventArgs e)
    {
        EditEmployeeErrorMessage.Text = string.Empty;
        EditEmployeeSuccessMessage.Text = string.Empty;
        EditEmployeeNewPassword.Text = string.Empty;
        EditEmployeeConfirmPassword.Text = string.Empty;

        int id;
        if (!int.TryParse(EditEmployeeDropDown.SelectedValue, out id))
        {
            EditEmployeeUserName.Text = string.Empty;
            return;
        }

        using (var db = new RepairShopContext())
        {
            var employee = db.Users.OfType<Employee>().FirstOrDefault(emp => emp.Id == id);
            EditEmployeeUserName.Text = employee != null ? employee.Name : string.Empty;
        }
    }

    protected void SaveEmployee_Click(object sender, EventArgs e)
    {
        int id;
        if (!int.TryParse(EditEmployeeDropDown.SelectedValue, out id))
        {
            EditEmployeeErrorMessage.Text = "Choose an employee first.";
            return;
        }

        if (string.IsNullOrWhiteSpace(EditEmployeeUserName.Text))
        {
            EditEmployeeErrorMessage.Text = "The user name field is required.";
            return;
        }

        if (EditEmployeeNewPassword.Text != EditEmployeeConfirmPassword.Text)
        {
            EditEmployeeErrorMessage.Text = "The password and confirmation password do not match.";
            return;
        }

        using (var db = new RepairShopContext())
        {
            var employee = db.Users.OfType<Employee>().FirstOrDefault(emp => emp.Id == id);
            if (employee == null)
            {
                EditEmployeeErrorMessage.Text = "That employee no longer exists.";
                BindEditEmployeeDropDown(db);
                return;
            }

            if (db.Users.Any(u => u.Name == EditEmployeeUserName.Text && u.Id != employee.Id && !u.DeletedAt.HasValue))
            {
                EditEmployeeErrorMessage.Text = "That username is already taken.";
                return;
            }

            employee.Name = EditEmployeeUserName.Text;

            if (!string.IsNullOrEmpty(EditEmployeeNewPassword.Text))
            {
                try
                {
                    employee.SetPassword(EditEmployeeNewPassword.Text);
                }
                catch (ArgumentException ex)
                {
                    EditEmployeeErrorMessage.Text = ex.Message;
                    return;
                }
            }

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException ex) when (IsDuplicateUsernameViolation(ex))
            {
                EditEmployeeErrorMessage.Text = "That username is already taken.";
                return;
            }

            var managerId = RepairAuthHelper.GetCurrentUserId();
            var manager = db.Users.OfType<Manager>().FirstOrDefault(m => m.Id == managerId);
            if (manager != null)
            {
                AuditLogger.Log(db, manager, AuditLog.ActionType.EditUser, AuditLog.EntityKind.User, employee.Id);
                db.SaveChanges();
            }

            EditEmployeeErrorMessage.Text = string.Empty;
            EditEmployeeSuccessMessage.Text = "Changes saved for " + employee.Name + ".";
            EditEmployeeNewPassword.Text = string.Empty;
            EditEmployeeConfirmPassword.Text = string.Empty;

            RefreshEmployeeViews(db);
        }
    }

    protected void DeleteEmployee_Click(object sender, EventArgs e)
    {
        int id;
        if (!int.TryParse(EditEmployeeDropDown.SelectedValue, out id))
        {
            EditEmployeeErrorMessage.Text = "Choose an employee first.";
            return;
        }

        using (var db = new RepairShopContext())
        {
            var employee = db.Users.OfType<Employee>().FirstOrDefault(emp => emp.Id == id);
            if (employee == null)
            {
                EditEmployeeErrorMessage.Text = "That employee no longer exists.";
                BindEditEmployeeDropDown(db);
                return;
            }

            // Soft delete only - their existing Tickets, Comments, and AuditLog entries
            // all keep pointing at this same User row, so history/attribution is
            // unaffected. This just hides them from active management (can't log in,
            // no longer offered for new assignments) rather than removing anything.
            employee.Delete();
            db.SaveChanges();

            var managerId = RepairAuthHelper.GetCurrentUserId();
            var manager = db.Users.OfType<Manager>().FirstOrDefault(m => m.Id == managerId);
            if (manager != null)
            {
                AuditLogger.Log(db, manager, AuditLog.ActionType.DeleteUser, AuditLog.EntityKind.User, employee.Id);
                db.SaveChanges();
            }

            EditEmployeeErrorMessage.Text = string.Empty;
            EditEmployeeSuccessMessage.Text = employee.Name + " has been removed.";
            EditEmployeeUserName.Text = string.Empty;
            EditEmployeeNewPassword.Text = string.Empty;
            EditEmployeeConfirmPassword.Text = string.Empty;

            RefreshEmployeeViews(db);
        }
    }

    private void BindEditCustomerDropDown(RepairShopContext db)
    {
        var customers = db.Users.OfType<Customer>().Where(c => !c.DeletedAt.HasValue).OrderBy(c => c.Name).ToList();

        EditCustomerDropDown.Items.Clear();
        EditCustomerDropDown.Items.Add(new ListItem("-- Select a customer --", ""));
        foreach (var customer in customers)
        {
            EditCustomerDropDown.Items.Add(new ListItem(customer.Name, customer.Id.ToString()));
        }
    }

    protected void AddCustomer_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NewCustomerUserName.Text))
        {
            AddCustomerErrorMessage.Text = "The user name field is required.";
            return;
        }

        if (NewCustomerPassword.Text != NewCustomerConfirmPassword.Text)
        {
            AddCustomerErrorMessage.Text = "The password and confirmation password do not match.";
            return;
        }

        using (var db = new RepairShopContext())
        {
            if (db.Users.Any(u => u.Name == NewCustomerUserName.Text && !u.DeletedAt.HasValue))
            {
                AddCustomerErrorMessage.Text = "That username is already taken.";
                return;
            }

            var customer = new Customer
            {
                Name = NewCustomerUserName.Text,
                CreatedAt = DateTime.Now
            };

            try
            {
                customer.SetPassword(NewCustomerPassword.Text);
            }
            catch (ArgumentException ex)
            {
                AddCustomerErrorMessage.Text = ex.Message;
                return;
            }

            db.Users.Add(customer);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException ex) when (IsDuplicateUsernameViolation(ex))
            {
                AddCustomerErrorMessage.Text = "That username is already taken.";
                return;
            }

            var managerId = RepairAuthHelper.GetCurrentUserId();
            var manager = db.Users.OfType<Manager>().FirstOrDefault(m => m.Id == managerId);
            if (manager != null)
            {
                AuditLogger.Log(db, manager, AuditLog.ActionType.CreateUser, AuditLog.EntityKind.User, customer.Id);
                db.SaveChanges();
            }

            AddCustomerErrorMessage.Text = string.Empty;
            AddCustomerSuccessMessage.Text = "Customer account created for " + customer.Name + ".";
            NewCustomerUserName.Text = string.Empty;
            NewCustomerPassword.Text = string.Empty;
            NewCustomerConfirmPassword.Text = string.Empty;

            BindEditCustomerDropDown(db);
        }
    }

    protected void EditCustomerDropDown_SelectedIndexChanged(object sender, EventArgs e)
    {
        EditCustomerErrorMessage.Text = string.Empty;
        EditCustomerSuccessMessage.Text = string.Empty;
        EditCustomerNewPassword.Text = string.Empty;
        EditCustomerConfirmPassword.Text = string.Empty;

        int id;
        if (!int.TryParse(EditCustomerDropDown.SelectedValue, out id))
        {
            EditCustomerUserName.Text = string.Empty;
            return;
        }

        using (var db = new RepairShopContext())
        {
            var customer = db.Users.OfType<Customer>().FirstOrDefault(c => c.Id == id);
            EditCustomerUserName.Text = customer != null ? customer.Name : string.Empty;
        }
    }

    protected void SaveCustomer_Click(object sender, EventArgs e)
    {
        int id;
        if (!int.TryParse(EditCustomerDropDown.SelectedValue, out id))
        {
            EditCustomerErrorMessage.Text = "Choose a customer first.";
            return;
        }

        if (string.IsNullOrWhiteSpace(EditCustomerUserName.Text))
        {
            EditCustomerErrorMessage.Text = "The user name field is required.";
            return;
        }

        if (EditCustomerNewPassword.Text != EditCustomerConfirmPassword.Text)
        {
            EditCustomerErrorMessage.Text = "The password and confirmation password do not match.";
            return;
        }

        using (var db = new RepairShopContext())
        {
            var customer = db.Users.OfType<Customer>().FirstOrDefault(c => c.Id == id);
            if (customer == null)
            {
                EditCustomerErrorMessage.Text = "That customer no longer exists.";
                BindEditCustomerDropDown(db);
                return;
            }

            if (db.Users.Any(u => u.Name == EditCustomerUserName.Text && u.Id != customer.Id && !u.DeletedAt.HasValue))
            {
                EditCustomerErrorMessage.Text = "That username is already taken.";
                return;
            }

            customer.Name = EditCustomerUserName.Text;

            if (!string.IsNullOrEmpty(EditCustomerNewPassword.Text))
            {
                try
                {
                    customer.SetPassword(EditCustomerNewPassword.Text);
                }
                catch (ArgumentException ex)
                {
                    EditCustomerErrorMessage.Text = ex.Message;
                    return;
                }
            }

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException ex) when (IsDuplicateUsernameViolation(ex))
            {
                EditCustomerErrorMessage.Text = "That username is already taken.";
                return;
            }

            var managerId = RepairAuthHelper.GetCurrentUserId();
            var manager = db.Users.OfType<Manager>().FirstOrDefault(m => m.Id == managerId);
            if (manager != null)
            {
                AuditLogger.Log(db, manager, AuditLog.ActionType.EditUser, AuditLog.EntityKind.User, customer.Id);
                db.SaveChanges();
            }

            EditCustomerErrorMessage.Text = string.Empty;
            EditCustomerSuccessMessage.Text = "Changes saved for " + customer.Name + ".";
            EditCustomerNewPassword.Text = string.Empty;
            EditCustomerConfirmPassword.Text = string.Empty;

            BindEditCustomerDropDown(db);
        }
    }

    protected void DeleteCustomer_Click(object sender, EventArgs e)
    {
        int id;
        if (!int.TryParse(EditCustomerDropDown.SelectedValue, out id))
        {
            EditCustomerErrorMessage.Text = "Choose a customer first.";
            return;
        }

        using (var db = new RepairShopContext())
        {
            var customer = db.Users.OfType<Customer>().FirstOrDefault(c => c.Id == id);
            if (customer == null)
            {
                EditCustomerErrorMessage.Text = "That customer no longer exists.";
                BindEditCustomerDropDown(db);
                return;
            }

            // Soft delete only - see the matching comment on DeleteEmployee_Click: their
            // existing Tickets, Comments, and AuditLog entries all keep pointing at this
            // same User row, so history/attribution is unaffected.
            customer.Delete();
            db.SaveChanges();

            var managerId = RepairAuthHelper.GetCurrentUserId();
            var manager = db.Users.OfType<Manager>().FirstOrDefault(m => m.Id == managerId);
            if (manager != null)
            {
                AuditLogger.Log(db, manager, AuditLog.ActionType.DeleteUser, AuditLog.EntityKind.User, customer.Id);
                db.SaveChanges();
            }

            EditCustomerErrorMessage.Text = string.Empty;
            EditCustomerSuccessMessage.Text = customer.Name + " has been removed.";
            EditCustomerUserName.Text = string.Empty;
            EditCustomerNewPassword.Text = string.Empty;
            EditCustomerConfirmPassword.Text = string.Empty;

            BindEditCustomerDropDown(db);
        }
    }
}
