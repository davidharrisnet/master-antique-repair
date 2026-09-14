using System;

public partial class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        var isAuthenticated = Context.User.Identity.IsAuthenticated;
        AnonymousCta.Visible = !isAuthenticated;
        AuthenticatedCta.Visible = isAuthenticated;

        if (!isAuthenticated)
        {
            return;
        }

        if (Context.User.IsInRole("Manager"))
        {
            MyViewLink.NavigateUrl = "~/ManagerView";
            MyViewLink.Text = "Go to Employees & Jobs";
        }
        else if (Context.User.IsInRole("Employee"))
        {
            MyViewLink.NavigateUrl = "~/EmployeeView";
            MyViewLink.Text = "Go to My Tickets";
        }
        else if (Context.User.IsInRole("Customer"))
        {
            MyViewLink.NavigateUrl = "~/CustomerView";
            MyViewLink.Text = "Go to My Repairs";
        }
    }
}
