<%@ Page Title="My Repairs" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="CustomerView.aspx.cs" Inherits="CustomerView" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <h2><%: Title %>.</h2>

    <p><a runat="server" href="~/SubmitRepair" class="btn btn-default">Submit a new repair</a></p>

    <h3>My Repair Requests</h3>
    <asp:GridView runat="server" ID="MyOrdersGrid" AutoGenerateColumns="false" CssClass="table">
        <Columns>
            <asp:BoundField DataField="Id" HeaderText="Id" />
            <asp:BoundField DataField="Description" HeaderText="Description" />
            <asp:BoundField DataField="State" HeaderText="Status" />
            <asp:BoundField DataField="SubmittedDate" HeaderText="Submitted" DataFormatString="{0:g}" />
        </Columns>
        <EmptyDataTemplate>You haven't submitted any repair requests yet.</EmptyDataTemplate>
    </asp:GridView>
</asp:Content>
