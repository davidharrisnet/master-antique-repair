using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MasterAntiqueRepair;

public partial class TicketDetailView : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!RepairAuthHelper.RequireRole(Response, "Manager"))
        {
            return;
        }

        if (!IsPostBack)
        {
            BindTicketList();
            BindCustomerList();
            BindEmployeeList();

            var idParam = Request.QueryString["id"];
            int id;
            if (!string.IsNullOrEmpty(idParam) && int.TryParse(idParam, out id))
            {
                TicketIdText.Text = id.ToString();
                SelectInDropDown(TicketIdDropDown, id);
                ShowTicket(id);
            }

            var customerIdParam = Request.QueryString["customerId"];
            int customerId;
            if (!string.IsNullOrEmpty(customerIdParam) && int.TryParse(customerIdParam, out customerId))
            {
                CustomerIdText.Text = customerId.ToString();
                SelectInDropDown(CustomerIdDropDown, customerId);
                ShowCustomer(customerId);
            }

            var employeeIdParam = Request.QueryString["employeeId"];
            int employeeId;
            if (!string.IsNullOrEmpty(employeeIdParam) && int.TryParse(employeeIdParam, out employeeId))
            {
                EmployeeIdText.Text = employeeId.ToString();
                SelectInDropDown(EmployeeIdDropDown, employeeId);
                ShowEmployee(employeeId);
            }
        }
    }

    private static string BuildPersonLink(int id, string name, string queryParam)
    {
        return "<a href=\"TicketDetailView.aspx?" + queryParam + "=" + id + "\">" + HttpUtility.HtmlEncode(name) + "</a>";
    }

    private static void SelectInDropDown(DropDownList dropDown, int id)
    {
        var item = dropDown.Items.FindByValue(id.ToString());
        if (item != null)
        {
            dropDown.ClearSelection();
            item.Selected = true;
        }
    }

    private void BindTicketList()
    {
        using (var db = new RepairShopContext())
        {
            var tickets = db.Tickets.OrderBy(t => t.Id).ToList();

            TicketIdDropDown.Items.Clear();
            TicketIdDropDown.Items.Add(new ListItem("-- Select a ticket --", ""));
            foreach (var ticket in tickets)
            {
                var description = ticket.Description ?? "";
                if (description.Length > 40)
                {
                    description = description.Substring(0, 40) + "...";
                }
                TicketIdDropDown.Items.Add(new ListItem("#" + ticket.Id + " - " + description, ticket.Id.ToString()));
            }
        }
    }

    protected void TicketIdDropDown_SelectedIndexChanged(object sender, EventArgs e)
    {
        int id;
        if (!int.TryParse(TicketIdDropDown.SelectedValue, out id))
        {
            return;
        }

        TicketIdText.Text = id.ToString();
        ShowTicket(id);
    }

    protected void Search_Click(object sender, EventArgs e)
    {
        int id;
        if (int.TryParse(TicketIdText.Text, out id))
        {
            SelectInDropDown(TicketIdDropDown, id);
            ShowTicket(id);
        }
        else
        {
            TicketPanel.Visible = false;
            NotFoundPanel.Visible = true;
        }
    }

    private void BindCustomerList()
    {
        using (var db = new RepairShopContext())
        {
            var customers = db.Users.OfType<Customer>().OrderBy(c => c.Id).ToList();

            CustomerIdDropDown.Items.Clear();
            CustomerIdDropDown.Items.Add(new ListItem("-- Select a customer --", ""));
            foreach (var customer in customers)
            {
                CustomerIdDropDown.Items.Add(new ListItem("#" + customer.Id + " - " + customer.Name, customer.Id.ToString()));
            }
        }
    }

    protected void CustomerIdDropDown_SelectedIndexChanged(object sender, EventArgs e)
    {
        int id;
        if (!int.TryParse(CustomerIdDropDown.SelectedValue, out id))
        {
            return;
        }

        CustomerIdText.Text = id.ToString();
        ShowCustomer(id);
    }

    protected void CustomerSearch_Click(object sender, EventArgs e)
    {
        int id;
        if (int.TryParse(CustomerIdText.Text, out id))
        {
            SelectInDropDown(CustomerIdDropDown, id);
            ShowCustomer(id);
        }
        else
        {
            CustomerPanel.Visible = false;
            CustomerNotFoundPanel.Visible = true;
        }
    }

    private void ShowCustomer(int id)
    {
        using (var db = new RepairShopContext())
        {
            var customer = db.Users.OfType<Customer>().FirstOrDefault(c => c.Id == id);
            if (customer == null)
            {
                CustomerPanel.Visible = false;
                CustomerNotFoundPanel.Visible = true;
                return;
            }

            CustomerNotFoundPanel.Visible = false;
            CustomerPanel.Visible = true;

            CustomerIdLiteral.Text = customer.Id.ToString();
            CustomerNameLiteral.Text = customer.Name;
            CustomerCreatedLiteral.Text = customer.CreatedAt.ToString("g");

            // Customer doesn't have its own Tickets collection - the inherited User.Tickets
            // maps to Ticket.User (the assigned employee), a separate relationship from
            // Ticket.Customer (who submitted it). Look submitted tickets up directly, same
            // workaround as Manager.GetCustomersWithTickets.
            var tickets = db.Tickets.Where(t => t.Customer.Id == id).OrderBy(t => t.Id).ToList();
            CustomerTicketsRepeater.DataSource = tickets;
            CustomerTicketsRepeater.DataBind();
            NoCustomerTicketsLabel.Visible = tickets.Count == 0;
        }
    }

    private void BindEmployeeList()
    {
        using (var db = new RepairShopContext())
        {
            var employees = db.Users.OfType<Employee>().OrderBy(emp => emp.Id).ToList();

            EmployeeIdDropDown.Items.Clear();
            EmployeeIdDropDown.Items.Add(new ListItem("-- Select an employee --", ""));
            foreach (var employee in employees)
            {
                EmployeeIdDropDown.Items.Add(new ListItem("#" + employee.Id + " - " + employee.Name, employee.Id.ToString()));
            }
        }
    }

    protected void EmployeeIdDropDown_SelectedIndexChanged(object sender, EventArgs e)
    {
        int id;
        if (!int.TryParse(EmployeeIdDropDown.SelectedValue, out id))
        {
            return;
        }

        EmployeeIdText.Text = id.ToString();
        ShowEmployee(id);
    }

    protected void EmployeeSearch_Click(object sender, EventArgs e)
    {
        int id;
        if (int.TryParse(EmployeeIdText.Text, out id))
        {
            SelectInDropDown(EmployeeIdDropDown, id);
            ShowEmployee(id);
        }
        else
        {
            EmployeePanel.Visible = false;
            EmployeeNotFoundPanel.Visible = true;
        }
    }

    private void ShowEmployee(int id)
    {
        using (var db = new RepairShopContext())
        {
            var employee = db.Users.OfType<Employee>().FirstOrDefault(emp => emp.Id == id);
            if (employee == null)
            {
                EmployeePanel.Visible = false;
                EmployeeNotFoundPanel.Visible = true;
                return;
            }

            EmployeeNotFoundPanel.Visible = false;
            EmployeePanel.Visible = true;

            EmployeeIdLiteral.Text = employee.Id.ToString();
            EmployeeNameLiteral.Text = employee.Name;
            EmployeeCreatedLiteral.Text = employee.CreatedAt.ToString("g");

            var tickets = employee.Tickets.OrderBy(t => t.Id).ToList();
            EmployeeTicketsRepeater.DataSource = tickets;
            EmployeeTicketsRepeater.DataBind();
            NoEmployeeTicketsLabel.Visible = tickets.Count == 0;
        }
    }

    private void ShowTicket(int id)
    {
        using (var db = new RepairShopContext())
        {
            var ticket = db.Tickets
                .Include(t => t.Customer)
                .Include(t => t.User)
                .FirstOrDefault(t => t.Id == id);
            if (ticket == null)
            {
                TicketPanel.Visible = false;
                NotFoundPanel.Visible = true;
                return;
            }

            NotFoundPanel.Visible = false;
            TicketPanel.Visible = true;

            TicketIdLiteral.Text = ticket.Id.ToString();
            DescriptionLiteral.Text = ticket.Description;
            StateLiteral.Text = ticket.State.ToString();

            CustomerLiteral.Text = ticket.Customer != null
                ? BuildPersonLink(ticket.Customer.Id, ticket.Customer.Name, "customerId")
                : "(none)";

            AssignedToLiteral.Text = ticket.User != null
                ? BuildPersonLink(ticket.User.Id, ticket.User.Name, "employeeId")
                : "(unassigned)";

            EmployeeCommentsAuthorLiteral.Text = ticket.User != null
                ? "(" + BuildPersonLink(ticket.User.Id, ticket.User.Name, "employeeId") + ")"
                : "";

            CustomerCommentsAuthorLiteral.Text = ticket.Customer != null
                ? "(" + BuildPersonLink(ticket.Customer.Id, ticket.Customer.Name, "customerId") + ")"
                : "";

            SubmittedLiteral.Text = ticket.SubmittedDate.HasValue ? ticket.SubmittedDate.Value.ToString("g") : "";
            AssignedLiteral.Text = ticket.AssignedDate.HasValue ? ticket.AssignedDate.Value.ToString("g") : "";
            CompletedLiteral.Text = ticket.CompletedDate.HasValue ? ticket.CompletedDate.Value.ToString("g") : "";

            var employeeComments = ticket.Comments.Where(c => c.User is Employee).OrderBy(c => c.CreatedAt).ToList();
            EmployeeCommentsRepeater.DataSource = employeeComments;
            EmployeeCommentsRepeater.DataBind();
            NoEmployeeCommentsLabel.Visible = employeeComments.Count == 0;

            var customerComments = ticket.Comments.Where(c => c.User is Customer).OrderBy(c => c.CreatedAt).ToList();
            CustomerCommentsRepeater.DataSource = customerComments;
            CustomerCommentsRepeater.DataBind();
            NoCustomerCommentsLabel.Visible = customerComments.Count == 0;
        }
    }
}
