using System;
using System.Linq;
using System.Web.UI;
using MasterAntiqueRepair;

public partial class Account_Register : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            RepairAuthHelper.RequireRole(Response, "Manager");
        }
    }

    protected void CreateUser_Click(object sender, EventArgs e)
    {
        if (!RepairAuthHelper.RequireRole(Response, "Manager"))
        {
            return;
        }

        using (var db = new RepairShopContext())
        {
            if (db.Users.Any(u => u.Name == UserName.Text))
            {
                ErrorMessage.Text = "That username is already taken.";
                return;
            }

            var employee = new Employee
            {
                Name = UserName.Text,
                CreatedAt = DateTime.Now
            };

            try
            {
                employee.SetPassword(Password.Text);
            }
            catch (ArgumentException ex)
            {
                ErrorMessage.Text = ex.Message;
                return;
            }

            db.Users.Add(employee);
            db.SaveChanges();

            SuccessMessage.Text = "Employee account created for " + employee.Name + ".";
            UserName.Text = string.Empty;
            Password.Text = string.Empty;
            ConfirmPassword.Text = string.Empty;
        }
    }
}
