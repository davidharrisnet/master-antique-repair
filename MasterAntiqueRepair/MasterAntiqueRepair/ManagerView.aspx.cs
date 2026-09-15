using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using MasterAntiqueRepair;

public partial class ManagerView : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!RepairAuthHelper.RequireRole(Response, "Manager"))
        {
            return;
        }

        if (!IsPostBack)
        {
            using (var db = new RepairShopContext())
            {
                var manager = new Manager();
                var employees = manager.GetEmployeesWithTickets(db);
                EmployeesRepeater.DataSource = employees;
                EmployeesRepeater.DataBind();
               
                UnassignedGrid.DataSource = db.Tickets
                    .Include(o => o.Customer)
                    .Where(o => o.User == null)
                    .OrderBy(o => o.Id)
                    .ToList();
                UnassignedGrid.DataBind();
            }
        }
    }

    protected void EmployeesRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
        {
            return;
        }

        var employee = (Employee)e.Item.DataItem;
        var ticketsRepeater = (Repeater)e.Item.FindControl("TicketsRepeater");
        ticketsRepeater.DataSource = employee.Tickets;
        ticketsRepeater.DataBind();
    }
   
}
