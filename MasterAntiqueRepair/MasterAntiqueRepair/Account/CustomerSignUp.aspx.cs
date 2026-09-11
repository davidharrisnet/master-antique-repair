using System;
using System.Linq;
using System.Web.UI;
using MasterAntiqueRepair;

public partial class Account_CustomerSignUp : Page
{
    protected void SignUp_Click(object sender, EventArgs e)
    {
        using (var db = new RepairShopContext())
        {
            if (db.Users.Any(u => u.Name == UserName.Text))
            {
                ErrorMessage.Text = "That username is already taken.";
                return;
            }

            var customer = new Customer
            {
                Name = UserName.Text,
                CreatedAt = DateTime.Now
            };

            try
            {
                customer.SetPassword(Password.Text);
            }
            catch (ArgumentException ex)
            {
                ErrorMessage.Text = ex.Message;
                return;
            }

            db.Users.Add(customer);
            db.SaveChanges();

            RepairAuthHelper.SignIn(customer, isPersistent: false);
            Response.Redirect("~/CustomerView");
        }
    }
}
