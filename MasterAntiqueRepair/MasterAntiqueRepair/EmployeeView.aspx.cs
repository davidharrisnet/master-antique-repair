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
        using (var db = new RepairShopContext())
        {
            var employeeId = RepairAuthHelper.GetCurrentUserId();

            UnassignedGrid.DataSource = db.Tickets
                .Where(o => o.User == null)
                .OrderBy(o => o.Id)
                .ToList();
            UnassignedGrid.DataBind();

            MyTicketsGrid.DataSource = db.Tickets
                .Where(o => o.User != null && o.User.Id == employeeId)
                .OrderBy(o => o.Id)
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

        using (var db = new RepairShopContext())
        {
            var employee = db.Users.OfType<Employee>().FirstOrDefault(u => u.Id == employeeId);
            var ticket = db.Tickets.FirstOrDefault(t => t.Id == ticketId && t.User != null && t.User.Id == employeeId);

            if (employee != null && ticket != null)
            {
                try
                {
                    var addedComment = employee.AddComment(ticket, NewCommentText.Text);
                    db.SaveChanges();

                    AuditLogger.Log(db, employee, AuditLog.ActionType.AddComment, AuditLog.EntityKind.Comment, addedComment.Id);
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
        AddCommentTicketId.Value = string.Empty;
        CommentErrorLabel.Visible = false;
        BindGrids();
    }

    protected void EmployeeCommentsRepeater_ItemCommand(object sender, RepeaterCommandEventArgs e)
    {
        if (e.CommandName != "DeleteComment")
        {
            return;
        }

        var commentId = Convert.ToInt32(e.CommandArgument);
        var employeeId = RepairAuthHelper.GetCurrentUserId();

        using (var db = new RepairShopContext())
        {
            var employee = db.Users.OfType<Employee>().FirstOrDefault(u => u.Id == employeeId);
            var comment = db.Comments.FirstOrDefault(c => c.Id == commentId);

            if (employee != null && comment != null)
            {
                try
                {
                    employee.DeleteComment(comment);
                    AuditLogger.Log(db, employee, AuditLog.ActionType.DeleteComment, AuditLog.EntityKind.Comment, comment.Id);
                    db.Comments.Remove(comment);
                    db.SaveChanges();
                }
                catch (InvalidOperationException)
                {
                    // Not this employee's comment - ignore.
                }
            }
        }

        BindGrids();
    }

    protected void SaveEditComment_Click(object sender, EventArgs e)
    {
        int commentId;
        if (!int.TryParse(EditCommentId.Value, out commentId))
        {
            return;
        }

        var employeeId = RepairAuthHelper.GetCurrentUserId();

        using (var db = new RepairShopContext())
        {
            var employee = db.Users.OfType<Employee>().FirstOrDefault(u => u.Id == employeeId);
            var comment = db.Comments.FirstOrDefault(c => c.Id == commentId);

            if (employee != null && comment != null)
            {
                try
                {
                    employee.EditComment(comment, EditCommentText.Text);
                    AuditLogger.Log(db, employee, AuditLog.ActionType.EditComment, AuditLog.EntityKind.Comment, comment.Id);
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

        using (var db = new RepairShopContext())
        {
            var employee = db.Users.OfType<Employee>().FirstOrDefault(u => u.Id == employeeId);
            var ticket = db.Tickets.FirstOrDefault(o => o.Id == ticketId);

            if (employee != null && ticket != null && ticket.User == null)
            {
                employee.TakeTicket(ticket);
                AuditLogger.Log(db, employee, AuditLog.ActionType.AssignTicket, AuditLog.EntityKind.Ticket, ticket.Id);
                db.SaveChanges();
            }
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

        using (var db = new RepairShopContext())
        {
            var employee = db.Users.OfType<Employee>().FirstOrDefault(u => u.Id == employeeId);
            var ticket = db.Tickets.FirstOrDefault(o => o.Id == ticketId && o.User != null && o.User.Id == employeeId);

            if (employee != null && ticket != null)
            {
                employee.CompleteTicket(ticket, comment);
                AuditLogger.Log(db, employee, AuditLog.ActionType.CompleteTicket, AuditLog.EntityKind.Ticket, ticket.Id);
                db.SaveChanges();
            }
        }

        ModalCommentBox.Text = string.Empty;
        CompleteTicketId.Value = string.Empty;
        BindGrids();
    }
}
