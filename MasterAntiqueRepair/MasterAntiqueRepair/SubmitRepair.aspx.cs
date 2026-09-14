using System;
using System.Linq;
using System.Web.UI;
using MasterAntiqueRepair;

public partial class SubmitRepair : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        RepairAuthHelper.RequireRole(Response, "Customer");
    }

    protected void Submit_Click(object sender, EventArgs e)
    {
        if (!IsValid)
        {
            return;
        }

        if (!RepairAuthHelper.RequireRole(Response, "Customer"))
        {
            return;
        }

        var customerId = RepairAuthHelper.GetCurrentUserId();

        using (var db = new RepairShopContext())
        {
            var customer = db.Users.OfType<Customer>().FirstOrDefault(c => c.Id == customerId);
            if (customer == null)
            {
                return;
            }

            var ticket = Ticket.CreateSubmitted(Description.Text, customer);
            db.Tickets.Add(ticket);
            db.SaveChanges();
        }

        Response.Redirect("~/CustomerView");
    }
}
