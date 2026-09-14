using System;
using System.Linq;
using System.Web.UI;
using MasterAntiqueRepair;

public partial class Account_CustomerSignUp : Page
{
    protected void SignUp_Click(object sender, EventArgs e)
    {
        var ip = Request.UserHostAddress;
        if (IpThrottle.IsBlocked("signup", ip))
        {
            ErrorMessage.Text = "Too many sign-up attempts from this location. Please try again later.";
            return;
        }

        using (var db = new RepairShopContext())
        {
            if (db.Users.Any(u => u.Name == UserName.Text))
            {
                // Rate-limited (not message-obscured): usernames in this app aren't secret
                // (they're already visible in Manager views and the Audit Log), so the
                // meaningful defense here is capping how fast an IP can sweep through
                // candidate usernames, not hiding which ones are taken.
                IpThrottle.RecordAttempt("signup", ip);
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

            AuditLogger.Log(db, customer, AuditLog.ActionType.CreateUser, AuditLog.EntityKind.User, customer.Id);
            db.SaveChanges();

            RepairAuthHelper.SignIn(customer, isPersistent: false);
            Response.Redirect("~/CustomerView");
        }
    }
}
