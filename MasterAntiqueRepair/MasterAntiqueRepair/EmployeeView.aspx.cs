using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using MasterAntiqueRepair;

public partial class EmployeeView : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!RepairAuthHelper.RequireRole(Response, "Employee"))
        {
            return;
        }

        if (!IsPostBack)
        {
            BindGrids();
        }
    }

    private void BindGrids()
    {
        var employeeId = RepairAuthHelper.GetCurrentUserId();

        using (var service = new TicketService())
        {
            UnassignedGrid.DataSource = service.GetUnassigned();
            UnassignedGrid.DataBind();

            MyTicketsGrid.DataSource = service.GetAssignedTo(employeeId);
            MyTicketsGrid.DataBind();
        }
    }

    protected void MyTicketsGrid_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow)
        {
            return;
        }

        var ticket = (Ticket)e.Row.DataItem;

        var employeeCommentsRepeater = (Repeater)e.Row.FindControl("EmployeeCommentsRepeater");
        employeeCommentsRepeater.DataSource = ticket.Comments.Where(c => c.User is Employee).OrderBy(c => c.CreatedAt).ToList();
        employeeCommentsRepeater.DataBind();

        var customerCommentsRepeater = (Repeater)e.Row.FindControl("CustomerCommentsRepeater");
        customerCommentsRepeater.DataSource = ticket.Comments.Where(c => c.User is Customer).OrderBy(c => c.CreatedAt).ToList();
        customerCommentsRepeater.DataBind();
    }

    protected void PostComment_Click(object sender, EventArgs e)
    {
        int ticketId;
        if (!int.TryParse(AddCommentTicketId.Value, out ticketId))
        {
            return;
        }

        var employeeId = RepairAuthHelper.GetCurrentUserId();

        using (var service = new CommentService())
        {
            try
            {
                service.AddEmployeeComment(employeeId, ticketId, NewCommentText.Text);
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                CommentErrorLabel.Text = ex.Message;
                CommentErrorLabel.Visible = true;
                return;
            }
        }

        NewCommentText.Text = string.Empty;
        AddCommentTicketId.Value = string.Empty;
        CommentErrorLabel.Visible = false;
        BindGrids();
    }

    protected void UnassignedGrid_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName != "Take")
        {
            return;
        }

        var ticketId = Convert.ToInt32(e.CommandArgument);
        var employeeId = RepairAuthHelper.GetCurrentUserId();

        using (var service = new TicketService())
        {
            service.AssignToMe(ticketId, employeeId);
        }

        BindGrids();
    }

    protected void ConfirmComplete_Click(object sender, EventArgs e)
    {
        int ticketId;
        if (!int.TryParse(CompleteTicketId.Value, out ticketId))
        {
            return;
        }

        var employeeId = RepairAuthHelper.GetCurrentUserId();
        var comment = ModalCommentBox.Text;

        using (var service = new TicketService())
        {
            service.CompleteTicket(ticketId, employeeId, comment);
        }

        ModalCommentBox.Text = string.Empty;
        CompleteTicketId.Value = string.Empty;
        BindGrids();
    }
}
