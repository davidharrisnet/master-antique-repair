<%@ Page Title="Employees" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="ManagerView.aspx.cs" Inherits="ManagerView" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <h2>Employees</h2>

    <h3>Employees</h3>
    <asp:Repeater runat="server" ID="EmployeesRepeater" OnItemDataBound="EmployeesRepeater_ItemDataBound">
        <ItemTemplate>
            <div class="panel panel-default">
                <div class="panel-heading"><%#: Eval("Name") %></div>
                <div class="panel-body">
                    <table class="table table-condensed table-fixed">
                        <colgroup>
                            <col />
                            <col style="width : 120px" />
                        </colgroup>
                        <asp:Repeater runat="server" ID="TicketsRepeater">
                            <HeaderTemplate>
                                <thead><tr><th>Description</th><th>Status</th></tr></thead>
                                <tbody>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr>
                                    <td><%#: Eval("Description") %></td>
                                    <td><span class='label <%# MasterAntiqueRepair.UiHelpers.StatusLabelClass((MasterAntiqueRepair.State.RepairState)Eval("State")) %>'><%# Eval("State") %></span></td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate></tbody></FooterTemplate>
                        </asp:Repeater>
                    </table>
                </div>
            </div>
        </ItemTemplate>
    </asp:Repeater>

    <h3>Unassigned Tickets</h3>
    <asp:GridView runat="server" ID="UnassignedGrid" AutoGenerateColumns="false" CssClass="table">
        <Columns>
            <asp:BoundField DataField="Id" HeaderText="Id" />
            <asp:BoundField DataField="Description" HeaderText="Description" />
            <asp:BoundField DataField="Customer.Name" HeaderText="Customer" />
        </Columns>
        <EmptyDataTemplate>No unassigned tickets right now.</EmptyDataTemplate>
    </asp:GridView>
  
</asp:Content>
