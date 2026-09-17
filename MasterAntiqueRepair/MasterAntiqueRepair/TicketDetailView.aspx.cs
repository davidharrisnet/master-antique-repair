using System;
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
            using (var service = new SearchService())
            {
                BindTicketList(service);
                BindCustomerList(service);
                BindEmployeeList(service);

                var idParam = Request.QueryString["id"];
                int id;
                if (!string.IsNullOrEmpty(idParam) && int.TryParse(idParam, out id))
                {
                    TicketIdText.Text = id.ToString();
                    SelectInDropDown(TicketIdDropDown, id);
                    ShowTicket(service, id);
                }

                var customerIdParam = Request.QueryString["customerId"];
                int customerId;
                if (!string.IsNullOrEmpty(customerIdParam) && int.TryParse(customerIdParam, out customerId))
                {
                    CustomerIdText.Text = customerId.ToString();
                    SelectInDropDown(CustomerIdDropDown, customerId);
                    ShowCustomer(service, customerId);
                }

                var employeeIdParam = Request.QueryString["employeeId"];
                int employeeId;
                if (!string.IsNullOrEmpty(employeeIdParam) && int.TryParse(employeeIdParam, out employeeId))
                {
                    EmployeeIdText.Text = employeeId.ToString();
                    SelectInDropDown(EmployeeIdDropDown, employeeId);
                    ShowEmployee(service, employeeId);
                }
            }
        }

        SetActiveTab(DetermineInitialTab());
    }

    // Which left-column tab should start active, based on which cross-link (if any)
    // brought the visitor here - e.g. a customer/employee id link from another page
    // should land straight on that search, not always default back to Ticket Search.
    private string DetermineInitialTab()
    {
        if (!string.IsNullOrEmpty(Request.QueryString["employeeId"]))
        {
            return "employee";
        }
        if (!string.IsNullOrEmpty(Request.QueryString["customerId"]))
        {
            return "customer";
        }
        return "ticket";
    }

    private void SetActiveTab(string tab)
    {
        TicketSearchTabItem.Attributes["class"] = tab == "ticket" ? "active" : "";
        CustomerSearchTabItem.Attributes["class"] = tab == "customer" ? "active" : "";
        EmployeeSearchTabItem.Attributes["class"] = tab == "employee" ? "active" : "";
        CommentSearchTabItem.Attributes["class"] = tab == "comment" ? "active" : "";

        TicketSearchPane.Attributes["class"] = "tab-pane" + (tab == "ticket" ? " active" : "");
        CustomerSearchPane.Attributes["class"] = "tab-pane" + (tab == "customer" ? " active" : "");
        EmployeeSearchPane.Attributes["class"] = "tab-pane" + (tab == "employee" ? " active" : "");
        CommentSearchPane.Attributes["class"] = "tab-pane" + (tab == "comment" ? " active" : "");
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

    private void BindTicketList(SearchService service)
    {
        var tickets = service.GetTicketDropdownList();

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

    protected void TicketIdDropDown_SelectedIndexChanged(object sender, EventArgs e)
    {
        int id;
        if (!int.TryParse(TicketIdDropDown.SelectedValue, out id))
        {
            return;
        }

        TicketIdText.Text = id.ToString();
        using (var service = new SearchService())
        {
            ShowTicket(service, id);
        }
    }

    protected void Search_Click(object sender, EventArgs e)
    {
        int id;
        if (int.TryParse(TicketIdText.Text, out id))
        {
            SelectInDropDown(TicketIdDropDown, id);
            using (var service = new SearchService())
            {
                ShowTicket(service, id);
            }
        }
        else
        {
            TicketPanel.Visible = false;
            NotFoundPanel.Visible = true;
        }
    }

    private void BindCustomerList(SearchService service)
    {
        var customers = service.GetCustomerDropdownList();

        CustomerIdDropDown.Items.Clear();
        CustomerIdDropDown.Items.Add(new ListItem("-- Select a customer --", ""));
        foreach (var customer in customers)
        {
            var label = "#" + customer.Id + " - " + customer.Name + (customer.IsDeleted ? " (deleted)" : "");
            CustomerIdDropDown.Items.Add(new ListItem(label, customer.Id.ToString()));
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
        using (var service = new SearchService())
        {
            ShowCustomer(service, id);
        }
    }

    protected void CustomerSearch_Click(object sender, EventArgs e)
    {
        int id;
        if (int.TryParse(CustomerIdText.Text, out id))
        {
            SelectInDropDown(CustomerIdDropDown, id);
            using (var service = new SearchService())
            {
                ShowCustomer(service, id);
            }
        }
        else
        {
            CustomerPanel.Visible = false;
            CustomerNotFoundPanel.Visible = true;
        }
    }

    private void ShowCustomer(SearchService service, int id)
    {
        var customer = service.GetCustomerById(id);
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
        CustomerDeletedLabel.Visible = customer.IsDeleted;

        var tickets = service.GetTicketsForCustomer(id);
        CustomerTicketsRepeater.DataSource = tickets;
        CustomerTicketsRepeater.DataBind();
        NoCustomerTicketsLabel.Visible = tickets.Count == 0;
    }

    private void BindEmployeeList(SearchService service)
    {
        var employees = service.GetEmployeeDropdownList();

        EmployeeIdDropDown.Items.Clear();
        EmployeeIdDropDown.Items.Add(new ListItem("-- Select an employee --", ""));
        foreach (var employee in employees)
        {
            var label = "#" + employee.Id + " - " + employee.Name + (employee.IsDeleted ? " (deleted)" : "");
            EmployeeIdDropDown.Items.Add(new ListItem(label, employee.Id.ToString()));
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
        using (var service = new SearchService())
        {
            ShowEmployee(service, id);
        }
    }

    protected void EmployeeSearch_Click(object sender, EventArgs e)
    {
        int id;
        if (int.TryParse(EmployeeIdText.Text, out id))
        {
            SelectInDropDown(EmployeeIdDropDown, id);
            using (var service = new SearchService())
            {
                ShowEmployee(service, id);
            }
        }
        else
        {
            EmployeePanel.Visible = false;
            EmployeeNotFoundPanel.Visible = true;
        }
    }

    private void ShowEmployee(SearchService service, int id)
    {
        var employee = service.GetEmployeeById(id);
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
        EmployeeDeletedLabel.Visible = employee.IsDeleted;

        var tickets = service.GetTicketsForEmployee(id);
        EmployeeTicketsRepeater.DataSource = tickets;
        EmployeeTicketsRepeater.DataBind();
        NoEmployeeTicketsLabel.Visible = tickets.Count == 0;
    }

    private void ShowTicket(SearchService service, int id)
    {
        var ticket = service.GetTicketDetail(id);
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
        StateLiteral.Text = "<span class=\"label " + UiHelpers.StatusLabelClass(ticket.State) + "\">" + ticket.State + "</span>";

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

    protected void CommentSearch_Click(object sender, EventArgs e)
    {
        var searchText = CommentSearchText.Text.Trim();
        if (string.IsNullOrEmpty(searchText))
        {
            CommentSearchErrorPanel.Visible = true;
            CommentSearchErrorLiteral.Text = "Enter some text to search for.";
            CommentSearchRepeater.DataSource = null;
            CommentSearchRepeater.DataBind();
            NoCommentsFoundLabel.Visible = false;
            return;
        }

        CommentSearchErrorPanel.Visible = false;

        using (var service = new SearchService())
        {
            var results = service.SearchComments(searchText);

            CommentSearchRepeater.DataSource = results;
            CommentSearchRepeater.DataBind();
            NoCommentsFoundLabel.Visible = results.Count == 0;
        }
    }
}
