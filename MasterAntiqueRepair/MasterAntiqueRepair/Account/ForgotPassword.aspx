<%@ Page Title="Forgot Password" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="ForgotPassword.aspx.cs" Inherits="Account_ForgotPassword" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <h2><%: Title %>.</h2>

    <div class="row">
        <div class="col-md-8">
            <p>Enter your username and we'll generate a password reset link.</p>

            <asp:PlaceHolder runat="server" ID="ResultPanel" Visible="false">
                <p><asp:Literal runat="server" ID="ResultMessage" /></p>
                <asp:PlaceHolder runat="server" ID="ResetLinkPanel" Visible="false">
                    <p>
                        <asp:HyperLink runat="server" ID="ResetLinkHyperLink" CssClass="btn btn-default" />
                    </p>
                    <p class="text-muted">
                        <span class="glyphicon glyphicon-info-sign"></span>
                        This app has no email configured, so the reset link is shown here directly
                        instead of being emailed. This link expires in 1 hour and can only be used once.
                    </p>
                </asp:PlaceHolder>
            </asp:PlaceHolder>

            <div class="form-horizontal">
                <div class="form-group">
                    <asp:Label runat="server" AssociatedControlID="UserName" CssClass="col-md-3 control-label">User name</asp:Label>
                    <div class="col-md-9">
                        <asp:TextBox runat="server" ID="UserName" CssClass="form-control" autocomplete="username" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="UserName"
                            CssClass="text-danger" ErrorMessage="The user name field is required." />
                    </div>
                </div>
                <div class="form-group">
                    <div class="col-md-offset-3 col-md-9">
                        <asp:Button runat="server" OnClick="Submit_Click" Text="Request reset link" CssClass="btn btn-primary" />
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
