using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using MasterAntiqueRepair;

public partial class CustomerView : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!RepairAuthHelper.RequireRole(Response, "Customer"))
        {
            return;
        }

        if (!IsPostBack)
        {
            BindGrid();
        }
    }

    private void BindGrid()
    {
        var customerId = RepairAuthHelper.GetCurrentUserId();

        using (var service = new TicketService())
        {
            MyTicketsGrid.DataSource = service.GetMyTickets(customerId);
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

        var commentsRepeater = (Repeater)e.Row.FindControl("CommentsRepeater");
        commentsRepeater.DataSource = ticket.Comments.Where(c => c.User is Customer).OrderBy(c => c.CreatedAt).ToList();
        commentsRepeater.DataBind();
    }

    protected void PostComment_Click(object sender, EventArgs e)
    {
        int ticketId;
        if (!int.TryParse(CommentTicketId.Value, out ticketId))
        {
            return;
        }

        var customerId = RepairAuthHelper.GetCurrentUserId();

        using (var service = new CommentService())
        {
            try
            {
                service.AddCustomerComment(customerId, ticketId, NewCommentText.Text);
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                CommentErrorLabel.Text = ex.Message;
                CommentErrorLabel.Visible = true;
                return;
            }
        }

        NewCommentText.Text = string.Empty;
        CommentTicketId.Value = string.Empty;
        CommentErrorLabel.Visible = false;
        BindGrid();
    }
}
