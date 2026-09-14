using System;
using System.Linq;
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

        var ip = Request.UserHostAddress;
        if (IpThrottle.IsBlocked("forgotpassword", ip))
        {
            ResultPanel.Visible = true;
            ResetLinkPanel.Visible = false;
            ResultMessage.Text = "Too many requests from this location. Please try again later.";
            return;
        }
        IpThrottle.RecordAttempt("forgotpassword", ip);

        using (var db = new RepairShopContext())
        {
            var user = db.Users.FirstOrDefault(u => u.Name == UserName.Text);

            ResultPanel.Visible = true;

            if (user == null)
            {
                ResetLinkPanel.Visible = false;
                ResultMessage.Text = "No account found with that username.";
                return;
            }

            var token = PasswordResetToken.Create(user);
            db.PasswordResetTokens.Add(token);
            db.SaveChanges();

            AuditLogger.Log(db, user, AuditLog.ActionType.RequestPasswordReset, AuditLog.EntityKind.User, user.Id);
            db.SaveChanges();

            var resetUrl = new Uri(Request.Url, ResolveUrl("~/Account/ResetPassword") + "?token=" + HttpUtility.UrlEncode(token.Token)).AbsoluteUri;
            ResetLinkHyperLink.NavigateUrl = resetUrl;
            ResetLinkHyperLink.Text = resetUrl;
            ResetLinkPanel.Visible = true;
            ResultMessage.Text = "A reset link was generated for that account.";
        }
    }
}
