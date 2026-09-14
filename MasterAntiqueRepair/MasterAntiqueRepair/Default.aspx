<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron bg-primary text-center">
        <h1><span class="glyphicon glyphicon-wrench"></span> Master Antique Repair</h1>
        <p class="lead">Bring your antiques back to life. Submit a repair request and we'll take it from there.</p>

        <asp:Panel runat="server" ID="AnonymousCta">
            <a runat="server" href="~/Account/CustomerSignUp" class="btn btn-success btn-lg">Sign Up</a>
            <a runat="server" href="~/Account/Login" class="btn btn-default btn-lg">Log In</a>
        </asp:Panel>
        <asp:Panel runat="server" ID="AuthenticatedCta">
            <asp:HyperLink runat="server" ID="MyViewLink" CssClass="btn btn-success btn-lg" />
        </asp:Panel>
    </div>

    <div class="row text-center">
        <div class="col-md-4">
            <h2 class="text-primary"><span class="glyphicon glyphicon-inbox"></span></h2>
            <h4>Submit</h4>
            <p>Describe the item and the repair it needs.</p>
        </div>
        <div class="col-md-4">
            <h2 class="text-info"><span class="glyphicon glyphicon-wrench"></span></h2>
            <h4>Repair</h4>
            <p>An employee picks it up and gets to work.</p>
        </div>
        <div class="col-md-4">
            <h2 class="text-success"><span class="glyphicon glyphicon-ok-circle"></span></h2>
            <h4>Complete</h4>
            <p>Track status and leave comments once it's done.</p>
        </div>
    </div>
</asp:Content>
