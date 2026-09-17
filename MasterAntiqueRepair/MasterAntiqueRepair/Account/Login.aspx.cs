using System;
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
        if (!IsValid)
        {
            return;
        }

        User user;
        using (var service = new AuthService())
        {
            try
            {
                user = service.Login(UserName.Text, Password.Text, Request.UserHostAddress);
            }
            catch (InvalidOperationException ex)
            {
                FailureText.Text = ex.Message;
                ErrorMessage.Visible = true;
                return;
            }
        }

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
}
