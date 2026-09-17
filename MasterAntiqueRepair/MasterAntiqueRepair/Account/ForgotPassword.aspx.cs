using System;
using System.Web;
using System.Web.UI;
using MasterAntiqueRepair;

public partial class Account_ForgotPassword : Page
{
    protected void Submit_Click(object sender, EventArgs e)
    {
        if (!IsValid)
        {
            return;
        }

        PasswordResetRequest request;
        using (var service = new AuthService())
        {
            try
            {
                request = service.RequestPasswordReset(UserName.Text, Request.UserHostAddress);
            }
            catch (InvalidOperationException ex)
            {
                ResultPanel.Visible = true;
                ResetLinkPanel.Visible = false;
                ResultMessage.Text = ex.Message;
                return;
            }
        }

        ResultPanel.Visible = true;

        if (request == null)
        {
            ResetLinkPanel.Visible = false;
            ResultMessage.Text = "No account found with that username.";
            return;
        }

        var resetUrl = new Uri(Request.Url, ResolveUrl("~/Account/ResetPassword")
            + "?userId=" + request.UserId
            + "&code=" + HttpUtility.UrlEncode(request.Code)).AbsoluteUri;
        ResetLinkHyperLink.NavigateUrl = resetUrl;
        ResetLinkHyperLink.Text = resetUrl;
        ResetLinkPanel.Visible = true;
        ResultMessage.Text = "A reset link was generated for that account.";
    }
}
