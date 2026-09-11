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

            UnassignedGrid.DataSource = db.Orders
                .Where(o => o.User == null)
                .OrderBy(o => o.Id)
                .ToList();
            UnassignedGrid.DataBind();

            MyJobsGrid.DataSource = db.Orders
                .Where(o => o.User != null && o.User.Id == employeeId)
                .OrderBy(o => o.Id)
                .ToList();
            MyJobsGrid.DataBind();
        }
    }

    protected void UnassignedGrid_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName != "Take")
        {
            return;
        }

        var orderId = Convert.ToInt32(e.CommandArgument);
        var employeeId = RepairAuthHelper.GetCurrentUserId();

        using (var db = new RepairShopContext())
        {
            var employee = db.Users.OfType<Employee>().FirstOrDefault(u => u.Id == employeeId);
            var order = db.Orders.FirstOrDefault(o => o.Id == orderId);

            if (employee != null && order != null && order.User == null)
            {
                employee.TakeOrder(order);
                db.SaveChanges();
            }
        }

        BindGrids();
    }

    protected void ConfirmComplete_Click(object sender, EventArgs e)
    {
        int orderId;
        if (!int.TryParse(CompleteOrderId.Value, out orderId))
        {
            return;
        }

        var employeeId = RepairAuthHelper.GetCurrentUserId();
        var comment = ModalCommentBox.Text;

        using (var db = new RepairShopContext())
        {
            var employee = db.Users.OfType<Employee>().FirstOrDefault(u => u.Id == employeeId);
            var order = db.Orders.FirstOrDefault(o => o.Id == orderId && o.User != null && o.User.Id == employeeId);

            if (employee != null && order != null)
            {
                employee.CompleteOrder(order, comment);
                db.SaveChanges();
            }
        }

        ModalCommentBox.Text = string.Empty;
        CompleteOrderId.Value = string.Empty;
        BindGrids();
    }
}
