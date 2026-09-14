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

            Ticket ticket;
            try
            {
                ticket = Ticket.CreateSubmitted(Description.Text, customer);
            }
            catch (ArgumentException ex)
            {
                ErrorMessage.Text = ex.Message;
                ErrorMessage.Visible = true;
                return;
            }

            db.Tickets.Add(ticket);
            db.SaveChanges();

            AuditLogger.Log(db, customer, AuditLog.ActionType.CreateTicket, AuditLog.EntityKind.Ticket, ticket.Id);
            db.SaveChanges();
        }

        Response.Redirect("~/CustomerView");
    }
}
