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

        using (var db = new RepairShopContext())
        {
            MyTicketsGrid.DataSource = db.Tickets
                .Where(o => o.Customer != null && o.Customer.Id == customerId)
                .OrderByDescending(o => o.Id)
                .ToList();
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

        using (var db = new RepairShopContext())
        {
            var customer = db.Users.OfType<Customer>().FirstOrDefault(c => c.Id == customerId);
            var ticket = db.Tickets.FirstOrDefault(t => t.Id == ticketId && t.Customer != null && t.Customer.Id == customerId);

            if (customer != null && ticket != null)
            {
                try
                {
                    customer.AddComment(ticket, NewCommentText.Text);
                    db.SaveChanges();
                }
                catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
                {
                    CommentErrorLabel.Text = ex.Message;
                    CommentErrorLabel.Visible = true;
                    return;
                }
            }
        }

        NewCommentText.Text = string.Empty;
        CommentTicketId.Value = string.Empty;
        CommentErrorLabel.Visible = false;
        BindGrid();
    }

    protected void CommentsRepeater_ItemCommand(object sender, RepeaterCommandEventArgs e)
    {
        if (e.CommandName != "DeleteComment")
        {
            return;
        }

        var commentId = Convert.ToInt32(e.CommandArgument);
        var customerId = RepairAuthHelper.GetCurrentUserId();

        using (var db = new RepairShopContext())
        {
            var customer = db.Users.OfType<Customer>().FirstOrDefault(c => c.Id == customerId);
            var comment = db.Comments.FirstOrDefault(c => c.Id == commentId);

            if (customer != null && comment != null)
            {
                try
                {
                    customer.DeleteComment(comment);
                    db.Comments.Remove(comment);
                    db.SaveChanges();
                }
                catch (InvalidOperationException)
                {
                    // Not this customer's comment - ignore.
                }
            }
        }

        BindGrid();
    }

    protected void SaveEditComment_Click(object sender, EventArgs e)
    {
        int commentId;
        if (!int.TryParse(EditCommentId.Value, out commentId))
        {
            return;
        }

        var customerId = RepairAuthHelper.GetCurrentUserId();

        using (var db = new RepairShopContext())
        {
            var customer = db.Users.OfType<Customer>().FirstOrDefault(c => c.Id == customerId);
            var comment = db.Comments.FirstOrDefault(c => c.Id == commentId);

            if (customer != null && comment != null)
            {
                try
                {
                    customer.EditComment(comment, EditCommentText.Text);
                    db.SaveChanges();
                }
                catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
                {
                    EditCommentErrorLabel.Text = ex.Message;
                    EditCommentErrorLabel.Visible = true;
                    return;
                }
            }
        }

        EditCommentText.Text = string.Empty;
        EditCommentId.Value = string.Empty;
        EditCommentErrorLabel.Visible = false;
        BindGrid();
    }
}
