using System;
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

        using (var service = new TicketService())
        {
            Ticket ticket;
            try
            {
                ticket = service.SubmitTicket(customerId, Description.Text);
            }
            catch (ArgumentException ex)
            {
                ErrorMessage.Text = ex.Message;
                ErrorMessage.Visible = true;
                return;
            }

            if (ticket == null)
            {
                return;
            }
        }

        Response.Redirect("~/CustomerView");
    }
}
