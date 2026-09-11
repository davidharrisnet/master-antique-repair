using System;
using System.Linq;
using System.Web.UI;
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
            var customerId = RepairAuthHelper.GetCurrentUserId();

            using (var db = new RepairShopContext())
            {
                MyOrdersGrid.DataSource = db.Orders
                    .Where(o => o.Customer != null && o.Customer.Id == customerId)
                    .OrderByDescending(o => o.Id)
                    .ToList();
                MyOrdersGrid.DataBind();
            }
        }
    }
}
