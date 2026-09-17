<%@ Page Title="Reset Password" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="ResetPassword.aspx.cs" Inherits="Account_ResetPassword" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <h2><%: Title %>.</h2>

    <div class="row">
        <div class="col-md-8">
            <asp:PlaceHolder runat="server" ID="ErrorPanel" Visible="false">
                <p class="text-danger"><asp:Literal runat="server" ID="ErrorMessage" /></p>
                <p><a href="ForgotPassword">Request a new reset link</a></p>
            </asp:PlaceHolder>

            <asp:PlaceHolder runat="server" ID="SuccessPanel" Visible="false">
                <p class="text-success">
                    <span class="glyphicon glyphicon-ok"></span>
                    Your password has been reset. <a href="Login">Log in</a> with your new password.
                </p>
            </asp:PlaceHolder>

            <asp:PlaceHolder runat="server" ID="FormPanel">
                <asp:HiddenField runat="server" ID="UserIdHidden" />
                <asp:HiddenField runat="server" ID="CodeHidden" />
                <div class="form-horizontal">
                    <div class="form-group">
                        <asp:Label runat="server" AssociatedControlID="NewPassword" CssClass="col-md-3 control-label">New password</asp:Label>
                        <div class="col-md-9">
                            <asp:TextBox runat="server" ID="NewPassword" TextMode="Password" CssClass="form-control" autocomplete="new-password" />
                            <asp:RequiredFieldValidator runat="server" ControlToValidate="NewPassword"
                                CssClass="text-danger" ErrorMessage="The new password field is required." />
                        </div>
                    </div>
                    <div class="form-group">
                        <asp:Label runat="server" AssociatedControlID="ConfirmPassword" CssClass="col-md-3 control-label">Confirm password</asp:Label>
                        <div class="col-md-9">
                            <asp:TextBox runat="server" ID="ConfirmPassword" TextMode="Password" CssClass="form-control" autocomplete="new-password" />
                            <asp:RequiredFieldValidator runat="server" ControlToValidate="ConfirmPassword"
                                CssClass="text-danger" Display="Dynamic" ErrorMessage="The confirm password field is required." />
                            <asp:CompareValidator runat="server" ControlToCompare="NewPassword" ControlToValidate="ConfirmPassword"
                                CssClass="text-danger" Display="Dynamic" ErrorMessage="The password and confirmation password do not match." />
                        </div>
                    </div>
                    <div class="form-group">
                        <div class="col-md-offset-3 col-md-9">
                            <asp:Button runat="server" OnClick="ResetPassword_Click" Text="Reset password" CssClass="btn btn-primary" />
                        </div>
                    </div>
                </div>
            </asp:PlaceHolder>
        </div>
    </div>
</asp:Content>
