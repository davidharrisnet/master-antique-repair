using System;
using System.Web.UI;
using MasterAntiqueRepair;

public partial class Account_CustomerSignUp : Page
{
    protected void SignUp_Click(object sender, EventArgs e)
    {
        using (var service = new AuthService())
        {
            try
            {
                service.SignUp(UserName.Text, Password.Text, Request.UserHostAddress);
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                ErrorMessage.Text = ex.Message;
                return;
            }
        }

        Response.Redirect("~/CustomerView");
    }
}
