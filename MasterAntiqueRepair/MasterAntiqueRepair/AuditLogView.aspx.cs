using System;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using MasterAntiqueRepair;

public partial class AuditLogView : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!RepairAuthHelper.RequireRole(Response, "Manager"))
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
        using (var db = new RepairShopContext())
        {
            var query = db.AuditLogs.Include(a => a.User).AsQueryable();

            int entityId;
            if (int.TryParse(EntityIdSearchText.Text, out entityId))
            {
                query = query.Where(a => a.EntityId == entityId);
            }

            AuditLogGrid.DataSource = query.OrderByDescending(a => a.Timestamp).ToList();
            AuditLogGrid.DataBind();
        }
    }

    protected void AuditLogGrid_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow)
        {
            return;
        }

        var log = (AuditLog)e.Row.DataItem;
        var viewLink = (HyperLink)e.Row.FindControl("ViewLink");

        if (log.EntityType == AuditLog.EntityKind.Ticket)
        {
            viewLink.NavigateUrl = "~/TicketDetailView.aspx?id=" + log.EntityId;
            viewLink.Visible = true;
        }
        else if (log.EntityType == AuditLog.EntityKind.Comment)
        {
            using (var db = new RepairShopContext())
            {
                var comment = db.Comments.FirstOrDefault(c => c.Id == log.EntityId);
                if (comment != null)
                {
                    viewLink.NavigateUrl = "~/TicketDetailView.aspx?id=" + comment.TicketId;
                    viewLink.Visible = true;
                }
            }
        }
    }

    protected void AuditLogGrid_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        AuditLogGrid.PageIndex = e.NewPageIndex;
        BindGrid();
    }

    protected void PageSizeList_SelectedIndexChanged(object sender, EventArgs e)
    {
        AuditLogGrid.PageSize = int.Parse(PageSizeList.SelectedValue);
        AuditLogGrid.PageIndex = 0;
        BindGrid();
    }

    protected void Search_Click(object sender, EventArgs e)
    {
        AuditLogGrid.PageIndex = 0;
        BindGrid();
    }

    protected void ClearSearch_Click(object sender, EventArgs e)
    {
        EntityIdSearchText.Text = string.Empty;
        AuditLogGrid.PageIndex = 0;
        BindGrid();
    }
}
