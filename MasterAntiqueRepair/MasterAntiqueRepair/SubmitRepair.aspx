<%@ Page Title="Submit a Repair" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="SubmitRepair.aspx.cs" Inherits="SubmitRepair" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <h2><%: Title %>.</h2>

    <div class="form-horizontal">
        <h4>Tell us about the item you'd like repaired.</h4>
        <hr />
        <asp:ValidationSummary runat="server" CssClass="text-danger" />
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="Description" CssClass="col-md-2 control-label">Description</asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="Description" TextMode="MultiLine" Rows="4" CssClass="form-control" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="Description"
                    CssClass="text-danger" ErrorMessage="Please describe the item and the repair needed." />
            </div>
        </div>
        <div class="form-group">
            <div class="col-md-offset-2 col-md-10">
                <asp:Button runat="server" OnClick="Submit_Click" Text="Submit" CssClass="btn btn-default" />
            </div>
        </div>
    </div>
</asp:Content>
