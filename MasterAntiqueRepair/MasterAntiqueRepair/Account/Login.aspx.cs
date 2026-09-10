using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using MasterAntiqueRepair;

public partial class Account_Login : Page
{
        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterHyperLink.NavigateUrl = "Register";
            OpenAuthLogin.ReturnUrl = Request.QueryString["ReturnUrl"];
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
                using (var db = new RepairShopContext())
                {
                    var user = db.Users.FirstOrDefault(u => u.Name == UserName.Text);
                    if (user != null && user.VerifyPassword(Password.Text))
                    {
                        RepairAuthHelper.SignIn(user, RememberMe.Checked);
                        IdentityHelper.RedirectToReturnUrl(Request.QueryString["ReturnUrl"], Response);
                    }
                    else
                    {
                        FailureText.Text = "Invalid username or password.";
                        ErrorMessage.Visible = true;
                    }
                }
            }
        }
}