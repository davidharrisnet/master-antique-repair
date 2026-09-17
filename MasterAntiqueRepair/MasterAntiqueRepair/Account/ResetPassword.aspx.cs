using System;
using System.Web.UI;
using MasterAntiqueRepair;

public partial class Account_ResetPassword : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            var token = Request.QueryString["token"];
            TokenHidden.Value = token;

            using (var service = new AuthService())
            {
                if (!service.IsResetTokenValid(token))
                {
                    ShowInvalidToken();
                }
            }
        }
    }

    protected void ResetPassword_Click(object sender, EventArgs e)
    {
        if (!IsValid)
        {
            return;
        }

        var token = TokenHidden.Value;

        using (var service = new AuthService())
        {
            try
            {
                service.ResetPassword(token, NewPassword.Text);
            }
            catch (InvalidOperationException)
            {
                ShowInvalidToken();
                return;
            }
            catch (ArgumentException ex)
            {
                ErrorPanel.Visible = true;
                ErrorMessage.Text = ex.Message;
                return;
            }
        }

        FormPanel.Visible = false;
        SuccessPanel.Visible = true;
    }

    private void ShowInvalidToken()
    {
        FormPanel.Visible = false;
        ErrorPanel.Visible = true;
        ErrorMessage.Text = "This password reset link is invalid or has expired.";
    }
}
