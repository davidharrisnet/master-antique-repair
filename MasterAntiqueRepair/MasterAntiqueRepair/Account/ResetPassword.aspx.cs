using System;
using System.Web.UI;
using MasterAntiqueRepair;

public partial class Account_ResetPassword : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            int userId;
            int.TryParse(Request.QueryString["userId"], out userId);
            var code = Request.QueryString["code"];

            UserIdHidden.Value = userId.ToString();
            CodeHidden.Value = code;

            using (var service = new AuthService())
            {
                if (!service.IsResetTokenValid(userId, code))
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

        int userId;
        int.TryParse(UserIdHidden.Value, out userId);
        var code = CodeHidden.Value;

        using (var service = new AuthService())
        {
            try
            {
                service.ResetPassword(userId, code, NewPassword.Text);
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
