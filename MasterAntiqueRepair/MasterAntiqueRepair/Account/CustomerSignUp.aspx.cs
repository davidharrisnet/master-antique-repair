using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web.UI;
using MasterAntiqueRepair;

public partial class Account_CustomerSignUp : Page
{
    // Same race-condition backstop as ManagerView's Add/Edit handlers - the check
    // above is still a check-then-write, so this catches the database's own unique
    // index rejecting a duplicate that slipped past it.
    private static bool IsDuplicateUsernameViolation(DbUpdateException ex)
    {
        var sqlEx = ex.GetBaseException() as System.Data.SqlClient.SqlException;
        return sqlEx != null && (sqlEx.Number == 2601 || sqlEx.Number == 2627);
    }

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
            if (db.Users.Any(u => u.Name == UserName.Text && !u.DeletedAt.HasValue))
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

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException ex) when (IsDuplicateUsernameViolation(ex))
            {
                IpThrottle.RecordAttempt("signup", ip);
                ErrorMessage.Text = "That username is already taken.";
                return;
            }

            AuditLogger.Log(db, customer, AuditLog.ActionType.CreateUser, AuditLog.EntityKind.User, customer.Id);
            db.SaveChanges();

            RepairAuthHelper.SignIn(customer, isPersistent: false);
            Response.Redirect("~/CustomerView");
        }
    }
}
