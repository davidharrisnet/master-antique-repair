<%@ Page Title="Audit Log" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="AuditLogView.aspx.cs" Inherits="AuditLogView" MaintainScrollPositionOnPostBack="true" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <h2><%: Title %>.</h2>

    <div class="form-inline" style="margin-bottom: 15px;">
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="EntityIdSearchText">Entity Id</asp:Label>
            <asp:TextBox runat="server" ID="EntityIdSearchText" CssClass="form-control" />
        </div>
        <asp:Button runat="server" Text="Search" OnClick="Search_Click" CssClass="btn btn-primary" />
        <asp:Button runat="server" Text="Clear" OnClick="ClearSearch_Click" CssClass="btn btn-link" />

        <div class="form-group" style="margin-left: 20px;">
            <asp:Label runat="server" AssociatedControlID="PageSizeList">Rows per page</asp:Label>
            <asp:DropDownList runat="server" ID="PageSizeList" AutoPostBack="true" OnSelectedIndexChanged="PageSizeList_SelectedIndexChanged" CssClass="form-control">
                <asp:ListItem Text="10" Value="10" />
                <asp:ListItem Text="20" Value="20" />
            </asp:DropDownList>
        </div>
    </div>

    <asp:GridView runat="server" ID="AuditLogGrid" AutoGenerateColumns="false" CssClass="table"
        AllowPaging="true" PageSize="10" OnPageIndexChanging="AuditLogGrid_PageIndexChanging" OnRowDataBound="AuditLogGrid_RowDataBound">
        <Columns>
            <asp:BoundField DataField="Timestamp" HeaderText="Timestamp" DataFormatString="{0:g}" />
            <asp:BoundField DataField="User.Name" HeaderText="User" />
            <asp:BoundField DataField="Action" HeaderText="Action" />
            <asp:BoundField DataField="EntityType" HeaderText="Entity" />
            <asp:BoundField DataField="EntityId" HeaderText="Entity Id" />
            <asp:TemplateField HeaderText="">
                <ItemTemplate>
                    <asp:HyperLink runat="server" ID="ViewLink" Text="View" CssClass="btn btn-link btn-xs" Visible="false" />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <PagerSettings Mode="NumericFirstLast" PageButtonCount="10" FirstPageText="First" LastPageText="Last" />
        <PagerStyle CssClass="text-center" />
        <EmptyDataTemplate>No activity logged yet.</EmptyDataTemplate>
    </asp:GridView>
</asp:Content>
