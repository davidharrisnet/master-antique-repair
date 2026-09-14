using System;
using System.Linq;
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

            using (var db = new RepairShopContext())
            {
                var resetToken = db.PasswordResetTokens.FirstOrDefault(t => t.Token == token);
                if (resetToken == null || !resetToken.IsValid())
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

        using (var db = new RepairShopContext())
        {
            var resetToken = db.PasswordResetTokens.FirstOrDefault(t => t.Token == token);
            if (resetToken == null || !resetToken.IsValid())
            {
                ShowInvalidToken();
                return;
            }

            var user = db.Users.FirstOrDefault(u => u.Id == resetToken.UserId);
            if (user == null)
            {
                ShowInvalidToken();
                return;
            }

            try
            {
                user.SetPassword(NewPassword.Text);
            }
            catch (ArgumentException ex)
            {
                ErrorPanel.Visible = true;
                ErrorMessage.Text = ex.Message;
                return;
            }

            resetToken.UsedAt = DateTime.Now;
            user.RecordSuccessfulLogin(); // proving ownership via the emailed/shown link also clears any existing account lockout
            db.SaveChanges();

            AuditLogger.Log(db, user, AuditLog.ActionType.ResetPassword, AuditLog.EntityKind.User, user.Id);
            db.SaveChanges();

            FormPanel.Visible = false;
            SuccessPanel.Visible = true;
        }
    }

    private void ShowInvalidToken()
    {
        FormPanel.Visible = false;
        ErrorPanel.Visible = true;
        ErrorMessage.Text = "This password reset link is invalid or has expired.";
    }
}
