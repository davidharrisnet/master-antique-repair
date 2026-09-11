<%@ Page Title="Employees &amp; Jobs" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="ManagerView.aspx.cs" Inherits="ManagerView" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <h2>Employees &amp; Jobs</h2>

    <h3>Employees</h3>
    <asp:Repeater runat="server" ID="EmployeesRepeater" OnItemDataBound="EmployeesRepeater_ItemDataBound">
        <ItemTemplate>
            <div class="panel panel-default">
                <div class="panel-heading"><%# Eval("Name") %></div>
                <div class="panel-body">
                    <asp:Repeater runat="server" ID="OrdersRepeater">
                        <HeaderTemplate><ul></HeaderTemplate>
                        <ItemTemplate>
                            <li><%# Eval("Description") %> (<%# Eval("State") %>)</li>
                        </ItemTemplate>
                        <FooterTemplate></ul></FooterTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </ItemTemplate>
    </asp:Repeater>

    <h3>Unassigned Jobs</h3>
    <asp:GridView runat="server" ID="UnassignedGrid" AutoGenerateColumns="false" CssClass="table">
        <Columns>
            <asp:BoundField DataField="Id" HeaderText="Id" />
            <asp:BoundField DataField="Description" HeaderText="Description" />
        </Columns>
        <EmptyDataTemplate>No unassigned jobs right now.</EmptyDataTemplate>
    </asp:GridView>
</asp:Content>
