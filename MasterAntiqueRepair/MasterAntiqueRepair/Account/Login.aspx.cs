using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using MasterAntiqueRepair;

public partial class Account_Login : Page
{
        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterHyperLink.NavigateUrl = "CustomerSignUp";
            var returnUrl = HttpUtility.UrlEncode(Request.QueryString["ReturnUrl"]);
            if (!String.IsNullOrEmpty(returnUrl))
            {
                RegisterHyperLink.NavigateUrl += "?ReturnUrl=" + returnUrl;
            }
        }

        protected void LogIn(object sender, EventArgs e)
        {
            if (IsValid)
            {
                var ip = Request.UserHostAddress;
                if (IpThrottle.IsBlocked("login", ip))
                {
                    FailureText.Text = "Too many login attempts from this location. Please try again later.";
                    ErrorMessage.Visible = true;
                    return;
                }

                using (var db = new RepairShopContext())
                {
                    var user = db.Users.FirstOrDefault(u => u.Name == UserName.Text);

                    if (user != null && user.IsLockedOut())
                    {
                        IpThrottle.RecordAttempt("login", ip);
                        FailureText.Text = "This account is temporarily locked due to repeated failed login attempts. Please try again later.";
                        ErrorMessage.Visible = true;
                        return;
                    }

                    if (user != null && user.VerifyPassword(Password.Text))
                    {
                        user.RecordSuccessfulLogin();
                        db.SaveChanges();

                        AuditLogger.Log(db, user, AuditLog.ActionType.Login, AuditLog.EntityKind.User, user.Id);
                        db.SaveChanges();

                        RepairAuthHelper.SignIn(user, isPersistent: false);

                        var returnUrl = Request.QueryString["ReturnUrl"];
                        if (!String.IsNullOrEmpty(returnUrl))
                        {
                            IdentityHelper.RedirectToReturnUrl(returnUrl, Response);
                        }
                        else if (user is Employee)
                        {
                            Response.Redirect("~/EmployeeView");
                        }
                        else if (user is Manager)
                        {
                            Response.Redirect("~/ManagerView");
                        }
                        else if (user is Customer)
                        {
                            Response.Redirect("~/CustomerView");
                        }
                        else
                        {
                            Response.Redirect("~/");
                        }
                    }
                    else
                    {
                        IpThrottle.RecordAttempt("login", ip);

                        if (user != null)
                        {
                            user.RecordFailedLogin();
                            db.SaveChanges();
                        }

                        FailureText.Text = "Invalid username or password.";
                        ErrorMessage.Visible = true;
                    }
                }
            }
        }
}