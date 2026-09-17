using System;
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
        int entityId;
        int? entityIdFilter = int.TryParse(EntityIdSearchText.Text, out entityId) ? entityId : (int?)null;

        using (var service = new AuditLogService())
        {
            AuditLogGrid.DataSource = service.GetLogs(entityIdFilter);
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
            using (var service = new AuditLogService())
            {
                var ticketId = service.GetTicketIdForComment(log.EntityId);
                if (ticketId.HasValue)
                {
                    viewLink.NavigateUrl = "~/TicketDetailView.aspx?id=" + ticketId.Value;
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
