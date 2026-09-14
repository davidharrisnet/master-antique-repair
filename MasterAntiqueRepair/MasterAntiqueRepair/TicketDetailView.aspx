<%@ Page Title="Search" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="TicketDetailView.aspx.cs" Inherits="TicketDetailView" MaintainScrollPositionOnPostBack="true" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <h2><%: Title %>.</h2>

    <div class="row">
        <div class="col-sm-3">
            <ul class="nav nav-pills nav-stacked">
                <li runat="server" id="TicketSearchTabItem"><a href="#TicketSearchPane" data-toggle="pill">Ticket Search</a></li>
                <li runat="server" id="CustomerSearchTabItem"><a href="#CustomerSearchPane" data-toggle="pill">Customer Search</a></li>
                <li runat="server" id="EmployeeSearchTabItem"><a href="#EmployeeSearchPane" data-toggle="pill">Employee Search</a></li>
            </ul>
        </div>
        <div class="col-sm-9">
            <div class="tab-content">
    <div runat="server" id="TicketSearchPane" ClientIDMode="Static">
    <asp:UpdatePanel runat="server" ID="TicketSearchUpdatePanel" UpdateMode="Always">
        <ContentTemplate>
    <h3>Ticket Search</h3>
    <div class="form-inline" style="margin-bottom: 15px;">
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="TicketIdDropDown">Choose a ticket</asp:Label>
            <asp:DropDownList runat="server" ID="TicketIdDropDown" AutoPostBack="true" OnSelectedIndexChanged="TicketIdDropDown_SelectedIndexChanged" CssClass="form-control" />
        </div>
        <div class="form-group" style="margin-left: 20px;">
            <asp:Label runat="server" AssociatedControlID="TicketIdText">Or enter a Ticket Id</asp:Label>
            <asp:TextBox runat="server" ID="TicketIdText" CssClass="form-control" />
        </div>
        <asp:Button runat="server" Text="Search" OnClick="Search_Click" CssClass="btn btn-primary" />
    </div>

    <asp:Panel runat="server" ID="NotFoundPanel" Visible="false" CssClass="text-danger">
        No ticket found with that Id.
    </asp:Panel>

    <asp:Panel runat="server" ID="TicketPanel" Visible="false">
        <h3>Ticket #<asp:Literal runat="server" ID="TicketIdLiteral" Mode="Encode" /></h3>
        <dl class="dl-horizontal">
            <dt>Description</dt>
            <dd><asp:Literal runat="server" ID="DescriptionLiteral" Mode="Encode" /></dd>
            <dt>Status</dt>
            <dd><asp:Literal runat="server" ID="StateLiteral" /></dd>
            <dt>Customer</dt>
            <dd><asp:Literal runat="server" ID="CustomerLiteral" /></dd>
            <dt>Assigned To</dt>
            <dd><asp:Literal runat="server" ID="AssignedToLiteral" /></dd>
            <dt>Submitted</dt>
            <dd><asp:Literal runat="server" ID="SubmittedLiteral" Mode="Encode" /></dd>
            <dt>Assigned</dt>
            <dd><asp:Literal runat="server" ID="AssignedLiteral" Mode="Encode" /></dd>
            <dt>Completed</dt>
            <dd><asp:Literal runat="server" ID="CompletedLiteral" Mode="Encode" /></dd>
        </dl>

        <h4>Employee Comments <asp:Literal runat="server" ID="EmployeeCommentsAuthorLiteral" /></h4>
        <asp:Repeater runat="server" ID="EmployeeCommentsRepeater">
            <HeaderTemplate><ul></HeaderTemplate>
            <ItemTemplate>
                <li>
                    <span class="popover-toggle" tabindex="0" role="button" data-toggle="popover" data-trigger="focus" data-content='<%#: Eval("Text") %>'><%#: MasterAntiqueRepair.UiHelpers.Truncate(Eval("Text").ToString(), 60) %></span>
                    <small>(<%# Eval("CreatedAt", "{0:g}") %>)</small>
                </li>
            </ItemTemplate>
            <FooterTemplate></ul></FooterTemplate>
            <SeparatorTemplate></SeparatorTemplate>
        </asp:Repeater>
        <asp:Label runat="server" ID="NoEmployeeCommentsLabel" Text="(none)" Visible="false" />

        <h4>Customer Comments <asp:Literal runat="server" ID="CustomerCommentsAuthorLiteral" /></h4>
        <asp:Repeater runat="server" ID="CustomerCommentsRepeater">
            <HeaderTemplate><ul></HeaderTemplate>
            <ItemTemplate>
                <li>
                    <span class="popover-toggle" tabindex="0" role="button" data-toggle="popover" data-trigger="focus" data-content='<%#: Eval("Text") %>'><%#: MasterAntiqueRepair.UiHelpers.Truncate(Eval("Text").ToString(), 60) %></span>
                    <small>(<%# Eval("CreatedAt", "{0:g}") %>)</small>
                </li>
            </ItemTemplate>
            <FooterTemplate></ul></FooterTemplate>
        </asp:Repeater>
        <asp:Label runat="server" ID="NoCustomerCommentsLabel" Text="(none)" Visible="false" />
    </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
    </div>

    <div runat="server" id="CustomerSearchPane" ClientIDMode="Static">
    <asp:UpdatePanel runat="server" ID="CustomerSearchUpdatePanel" UpdateMode="Always">
        <ContentTemplate>
    <h3>Customer Search</h3>
    <div class="form-inline" style="margin-bottom: 15px;">
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="CustomerIdDropDown">Choose a customer</asp:Label>
            <asp:DropDownList runat="server" ID="CustomerIdDropDown" AutoPostBack="true" OnSelectedIndexChanged="CustomerIdDropDown_SelectedIndexChanged" CssClass="form-control" />
        </div>
        <div class="form-group" style="margin-left: 20px;">
            <asp:Label runat="server" AssociatedControlID="CustomerIdText">Or enter a Customer Id</asp:Label>
            <asp:TextBox runat="server" ID="CustomerIdText" CssClass="form-control" />
        </div>
        <asp:Button runat="server" Text="Search" OnClick="CustomerSearch_Click" CssClass="btn btn-primary" />
    </div>

    <asp:Panel runat="server" ID="CustomerNotFoundPanel" Visible="false" CssClass="text-danger">
        No customer found with that Id.
    </asp:Panel>

    <asp:Panel runat="server" ID="CustomerPanel" Visible="false">
        <h4>Customer #<asp:Literal runat="server" ID="CustomerIdLiteral" Mode="Encode" /></h4>
        <dl class="dl-horizontal">
            <dt>Name</dt>
            <dd><asp:Literal runat="server" ID="CustomerNameLiteral" Mode="Encode" /></dd>
            <dt>Created</dt>
            <dd><asp:Literal runat="server" ID="CustomerCreatedLiteral" Mode="Encode" /></dd>
        </dl>

        <h5>Submitted Tickets</h5>
        <table class="table table-condensed table-fixed">
            <colgroup>
                <col width="60" />
                <col />
                <col width="120" />
            </colgroup>
            <asp:Repeater runat="server" ID="CustomerTicketsRepeater">
                <HeaderTemplate>
                    <thead><tr><th>Id</th><th>Description</th><th>Status</th></tr></thead>
                    <tbody>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td><a href='<%# "TicketDetailView.aspx?id=" + Eval("Id") %>'>#<%# Eval("Id") %></a></td>
                        <td><%#: Eval("Description") %></td>
                        <td><span class='label <%# MasterAntiqueRepair.UiHelpers.StatusLabelClass((MasterAntiqueRepair.State.RepairState)Eval("State")) %>'><%# Eval("State") %></span></td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate></tbody></FooterTemplate>
            </asp:Repeater>
        </table>
        <asp:Label runat="server" ID="NoCustomerTicketsLabel" Text="(none)" Visible="false" />
    </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
    </div>

    <div runat="server" id="EmployeeSearchPane" ClientIDMode="Static">
    <asp:UpdatePanel runat="server" ID="EmployeeSearchUpdatePanel" UpdateMode="Always">
        <ContentTemplate>
    <h3>Employee Search</h3>
    <div class="form-inline" style="margin-bottom: 15px;">
        <div class="form-group">
            <asp:Label runat="server" AssociatedControlID="EmployeeIdDropDown">Choose an employee</asp:Label>
            <asp:DropDownList runat="server" ID="EmployeeIdDropDown" AutoPostBack="true" OnSelectedIndexChanged="EmployeeIdDropDown_SelectedIndexChanged" CssClass="form-control" />
        </div>
        <div class="form-group" style="margin-left: 20px;">
            <asp:Label runat="server" AssociatedControlID="EmployeeIdText">Or enter an Employee Id</asp:Label>
            <asp:TextBox runat="server" ID="EmployeeIdText" CssClass="form-control" />
        </div>
        <asp:Button runat="server" Text="Search" OnClick="EmployeeSearch_Click" CssClass="btn btn-primary" />
    </div>

    <asp:Panel runat="server" ID="EmployeeNotFoundPanel" Visible="false" CssClass="text-danger">
        No employee found with that Id.
    </asp:Panel>

    <asp:Panel runat="server" ID="EmployeePanel" Visible="false">
        <h4>Employee #<asp:Literal runat="server" ID="EmployeeIdLiteral" Mode="Encode" /></h4>
        <dl class="dl-horizontal">
            <dt>Name</dt>
            <dd><asp:Literal runat="server" ID="EmployeeNameLiteral" Mode="Encode" /></dd>
            <dt>Created</dt>
            <dd><asp:Literal runat="server" ID="EmployeeCreatedLiteral" Mode="Encode" /></dd>
        </dl>

        <h5>Assigned Tickets</h5>
        <table class="table table-condensed table-fixed">
            <colgroup>
                <col width="60" />
                <col />
                <col width="120" />
            </colgroup>
            <asp:Repeater runat="server" ID="EmployeeTicketsRepeater">
                <HeaderTemplate>
                    <thead><tr><th>Id</th><th>Description</th><th>Status</th></tr></thead>
                    <tbody>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td><a href='<%# "TicketDetailView.aspx?id=" + Eval("Id") %>'>#<%# Eval("Id") %></a></td>
                        <td><%#: Eval("Description") %></td>
                        <td><span class='label <%# MasterAntiqueRepair.UiHelpers.StatusLabelClass((MasterAntiqueRepair.State.RepairState)Eval("State")) %>'><%# Eval("State") %></span></td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate></tbody></FooterTemplate>
            </asp:Repeater>
        </table>
        <asp:Label runat="server" ID="NoEmployeeTicketsLabel" Text="(none)" Visible="false" />
    </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
    </div>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        function initPopovers() {
            $('[data-toggle="popover"]').popover();
        }

        $(function () {
            initPopovers();
            if (typeof (Sys) !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                    initPopovers();
                });
            }
        });
    </script>
</asp:Content>
