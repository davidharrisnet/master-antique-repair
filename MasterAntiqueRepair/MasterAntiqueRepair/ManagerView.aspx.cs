using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using MasterAntiqueRepair;

public partial class ManagerView : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!RepairAuthHelper.RequireRole(Response, "Manager"))
        {
            return;
        }

        if (!IsPostBack)
        {
            using (var ticketService = new TicketService())
            {
                UnassignedGrid.DataSource = ticketService.GetUnassignedWithCustomer();
                UnassignedGrid.DataBind();
            }

            using (var accountService = new AccountService())
            {
                BindEditEmployeeDropDown(accountService);
                BindViewEmployeeDropDown(accountService);
                BindEditCustomerDropDown(accountService);
            }
        }
    }

    private void BindEditEmployeeDropDown(AccountService service)
    {
        var employees = service.GetActiveEmployees();

        EditEmployeeDropDown.Items.Clear();
        EditEmployeeDropDown.Items.Add(new ListItem("-- Select an employee --", ""));
        foreach (var employee in employees)
        {
            EditEmployeeDropDown.Items.Add(new ListItem(employee.UserName, employee.Id.ToString()));
        }
    }

    private void BindViewEmployeeDropDown(AccountService service)
    {
        var employees = service.GetActiveEmployees();

        ViewEmployeeDropDown.Items.Clear();
        ViewEmployeeDropDown.Items.Add(new ListItem("-- Select an employee --", ""));
        foreach (var employee in employees)
        {
            ViewEmployeeDropDown.Items.Add(new ListItem(employee.UserName, employee.Id.ToString()));
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

        using (var accountService = new AccountService())
        using (var ticketService = new TicketService())
        {
            var employee = accountService.GetEmployeeById(id);
            if (employee == null)
            {
                ViewEmployeePanel.Visible = false;
                BindViewEmployeeDropDown(accountService);
                return;
            }

            ViewEmployeePanel.Visible = true;
            ViewEmployeeNameLiteral.Text = employee.UserName;

            var tickets = ticketService.GetAssignedTo(id);
            ViewEmployeeTicketsRepeater.DataSource = tickets;
            ViewEmployeeTicketsRepeater.DataBind();
            NoEmployeeTicketsLabel.Visible = tickets.Count == 0;
        }
    }

    private void RefreshEmployeeViews(AccountService service)
    {
        BindEditEmployeeDropDown(service);
        BindViewEmployeeDropDown(service);
    }

    protected void AddEmployee_Click(object sender, EventArgs e)
    {
        var managerId = RepairAuthHelper.GetCurrentUserId();

        using (var service = new AccountService())
        {
            Employee employee;
            try
            {
                employee = service.AddEmployee(managerId, NewEmployeeUserName.Text, NewEmployeePassword.Text, NewEmployeeConfirmPassword.Text);
            }
            catch (ArgumentException ex)
            {
                AddEmployeeErrorMessage.Text = ex.Message;
                return;
            }

            AddEmployeeErrorMessage.Text = string.Empty;
            AddEmployeeSuccessMessage.Text = "Employee account created for " + employee.UserName + ".";
            NewEmployeeUserName.Text = string.Empty;
            NewEmployeePassword.Text = string.Empty;
            NewEmployeeConfirmPassword.Text = string.Empty;

            RefreshEmployeeViews(service);
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

        using (var service = new AccountService())
        {
            var employee = service.GetEmployeeById(id);
            EditEmployeeUserName.Text = employee != null ? employee.UserName : string.Empty;
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

        var managerId = RepairAuthHelper.GetCurrentUserId();

        using (var service = new AccountService())
        {
            Employee employee;
            try
            {
                employee = service.EditEmployee(managerId, id, EditEmployeeUserName.Text, EditEmployeeNewPassword.Text, EditEmployeeConfirmPassword.Text);
            }
            catch (EntityNotFoundException ex)
            {
                EditEmployeeErrorMessage.Text = ex.Message;
                BindEditEmployeeDropDown(service);
                return;
            }
            catch (ArgumentException ex)
            {
                EditEmployeeErrorMessage.Text = ex.Message;
                return;
            }

            EditEmployeeErrorMessage.Text = string.Empty;
            EditEmployeeSuccessMessage.Text = "Changes saved for " + employee.UserName + ".";
            EditEmployeeNewPassword.Text = string.Empty;
            EditEmployeeConfirmPassword.Text = string.Empty;

            RefreshEmployeeViews(service);
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

        var managerId = RepairAuthHelper.GetCurrentUserId();

        using (var service = new AccountService())
        {
            Employee employee;
            try
            {
                employee = service.DeleteEmployee(managerId, id);
            }
            catch (EntityNotFoundException ex)
            {
                EditEmployeeErrorMessage.Text = ex.Message;
                BindEditEmployeeDropDown(service);
                return;
            }

            EditEmployeeErrorMessage.Text = string.Empty;
            EditEmployeeSuccessMessage.Text = employee.UserName + " has been removed.";
            EditEmployeeUserName.Text = string.Empty;
            EditEmployeeNewPassword.Text = string.Empty;
            EditEmployeeConfirmPassword.Text = string.Empty;

            RefreshEmployeeViews(service);
        }
    }

    private void BindEditCustomerDropDown(AccountService service)
    {
        var customers = service.GetActiveCustomers();

        EditCustomerDropDown.Items.Clear();
        EditCustomerDropDown.Items.Add(new ListItem("-- Select a customer --", ""));
        foreach (var customer in customers)
        {
            EditCustomerDropDown.Items.Add(new ListItem(customer.UserName, customer.Id.ToString()));
        }
    }

    protected void AddCustomer_Click(object sender, EventArgs e)
    {
        var managerId = RepairAuthHelper.GetCurrentUserId();

        using (var service = new AccountService())
        {
            Customer customer;
            try
            {
                customer = service.AddCustomer(managerId, NewCustomerUserName.Text, NewCustomerPassword.Text, NewCustomerConfirmPassword.Text);
            }
            catch (ArgumentException ex)
            {
                AddCustomerErrorMessage.Text = ex.Message;
                return;
            }

            AddCustomerErrorMessage.Text = string.Empty;
            AddCustomerSuccessMessage.Text = "Customer account created for " + customer.UserName + ".";
            NewCustomerUserName.Text = string.Empty;
            NewCustomerPassword.Text = string.Empty;
            NewCustomerConfirmPassword.Text = string.Empty;

            BindEditCustomerDropDown(service);
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

        using (var service = new AccountService())
        {
            var customer = service.GetCustomerById(id);
            EditCustomerUserName.Text = customer != null ? customer.UserName : string.Empty;
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

        var managerId = RepairAuthHelper.GetCurrentUserId();

        using (var service = new AccountService())
        {
            Customer customer;
            try
            {
                customer = service.EditCustomer(managerId, id, EditCustomerUserName.Text, EditCustomerNewPassword.Text, EditCustomerConfirmPassword.Text);
            }
            catch (EntityNotFoundException ex)
            {
                EditCustomerErrorMessage.Text = ex.Message;
                BindEditCustomerDropDown(service);
                return;
            }
            catch (ArgumentException ex)
            {
                EditCustomerErrorMessage.Text = ex.Message;
                return;
            }

            EditCustomerErrorMessage.Text = string.Empty;
            EditCustomerSuccessMessage.Text = "Changes saved for " + customer.UserName + ".";
            EditCustomerNewPassword.Text = string.Empty;
            EditCustomerConfirmPassword.Text = string.Empty;

            BindEditCustomerDropDown(service);
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

        var managerId = RepairAuthHelper.GetCurrentUserId();

        using (var service = new AccountService())
        {
            Customer customer;
            try
            {
                customer = service.DeleteCustomer(managerId, id);
            }
            catch (EntityNotFoundException ex)
            {
                EditCustomerErrorMessage.Text = ex.Message;
                BindEditCustomerDropDown(service);
                return;
            }

            EditCustomerErrorMessage.Text = string.Empty;
            EditCustomerSuccessMessage.Text = customer.UserName + " has been removed.";
            EditCustomerUserName.Text = string.Empty;
            EditCustomerNewPassword.Text = string.Empty;
            EditCustomerConfirmPassword.Text = string.Empty;

            BindEditCustomerDropDown(service);
        }
    }
}
