<%@ Page Title="Administration" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="ManagerView.aspx.cs" Inherits="ManagerView" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <h2>Administration</h2>

    <div class="panel panel-default">
        <div class="panel-heading" id="employeeManagementHeading">
            <h4 class="panel-title">
                <button type="button" class="panel-title-toggle" data-toggle="collapse" data-target="#employeeManagementBody" aria-expanded="false" aria-controls="employeeManagementBody">
                    <span>Employee Management</span>
                    <span class="glyphicon glyphicon-chevron-down"></span>
                </button>
            </h4>
        </div>
        <div id="employeeManagementBody" class="panel-collapse collapse" aria-labelledby="employeeManagementHeading">
        <div class="panel-body">
        <asp:UpdatePanel runat="server" ID="EmployeeManagementUpdatePanel" UpdateMode="Always">
            <ContentTemplate>
            <div class="row">
                <div class="col-md-6">
                    <h4>Add New Employee</h4>
                    <p class="text-danger"><asp:Literal runat="server" ID="AddEmployeeErrorMessage" /></p>
                    <p class="text-success"><asp:Literal runat="server" ID="AddEmployeeSuccessMessage" Mode="Encode" /></p>
                    <div class="form-horizontal">
                        <div class="form-group">
                            <asp:Label runat="server" AssociatedControlID="NewEmployeeUserName" CssClass="col-md-4 control-label">User name</asp:Label>
                            <div class="col-md-8">
                                <asp:TextBox runat="server" ID="NewEmployeeUserName" CssClass="form-control" autocomplete="username" />
                            </div>
                        </div>
                        <div class="form-group">
                            <asp:Label runat="server" AssociatedControlID="NewEmployeePassword" CssClass="col-md-4 control-label">Password</asp:Label>
                            <div class="col-md-8">
                                <asp:TextBox runat="server" ID="NewEmployeePassword" TextMode="Password" CssClass="form-control" autocomplete="new-password" />
                            </div>
                        </div>
                        <div class="form-group">
                            <asp:Label runat="server" AssociatedControlID="NewEmployeeConfirmPassword" CssClass="col-md-4 control-label">Confirm password</asp:Label>
                            <div class="col-md-8">
                                <asp:TextBox runat="server" ID="NewEmployeeConfirmPassword" TextMode="Password" CssClass="form-control" autocomplete="new-password" />
                            </div>
                        </div>
                        <div class="form-group">
                            <div class="col-md-offset-4 col-md-8">
                                <asp:Button runat="server" OnClick="AddEmployee_Click" Text="Add Employee" CssClass="btn btn-primary" />
                            </div>
                        </div>
                    </div>
                </div>

                <div class="col-md-6">
                    <h4>Edit Employee</h4>
                    <p class="text-danger"><asp:Literal runat="server" ID="EditEmployeeErrorMessage" /></p>
                    <p class="text-success"><asp:Literal runat="server" ID="EditEmployeeSuccessMessage" Mode="Encode" /></p>
                    <div class="form-horizontal">
                        <div class="form-group">
                            <asp:Label runat="server" AssociatedControlID="EditEmployeeDropDown" CssClass="col-md-4 control-label">Employee</asp:Label>
                            <div class="col-md-8">
                                <asp:DropDownList runat="server" ID="EditEmployeeDropDown" AutoPostBack="true" OnSelectedIndexChanged="EditEmployeeDropDown_SelectedIndexChanged" CssClass="form-control" />
                            </div>
                        </div>
                        <div class="form-group">
                            <asp:Label runat="server" AssociatedControlID="EditEmployeeUserName" CssClass="col-md-4 control-label">User name</asp:Label>
                            <div class="col-md-8">
                                <asp:TextBox runat="server" ID="EditEmployeeUserName" CssClass="form-control" autocomplete="username" />
                            </div>
                        </div>
                        <div class="form-group">
                            <asp:Label runat="server" AssociatedControlID="EditEmployeeNewPassword" CssClass="col-md-4 control-label">New password</asp:Label>
                            <div class="col-md-8">
                                <asp:TextBox runat="server" ID="EditEmployeeNewPassword" TextMode="Password" CssClass="form-control" autocomplete="new-password" />
                                <span class="help-block">Leave blank to keep the current password.</span>
                            </div>
                        </div>
                        <div class="form-group">
                            <asp:Label runat="server" AssociatedControlID="EditEmployeeConfirmPassword" CssClass="col-md-4 control-label">Confirm new password</asp:Label>
                            <div class="col-md-8">
                                <asp:TextBox runat="server" ID="EditEmployeeConfirmPassword" TextMode="Password" CssClass="form-control" autocomplete="new-password" />
                            </div>
                        </div>
                        <div class="form-group">
                            <div class="col-md-offset-4 col-md-8">
                                <asp:Button runat="server" OnClick="SaveEmployee_Click" Text="Save Changes" CssClass="btn btn-primary" />
                                <asp:Button runat="server" OnClick="DeleteEmployee_Click" Text="Delete Employee" CssClass="btn btn-danger"
                                    OnClientClick="return confirm('Are you sure? This employee will no longer be able to log in, but their existing tickets, comments, and history are kept.');" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            </ContentTemplate>
        </asp:UpdatePanel>
        </div>
        </div>
    </div>

    <div class="panel panel-default">
        <div class="panel-heading" id="customerManagementHeading">
            <h4 class="panel-title">
                <button type="button" class="panel-title-toggle" data-toggle="collapse" data-target="#customerManagementBody" aria-expanded="false" aria-controls="customerManagementBody">
                    <span>Customer Management</span>
                    <span class="glyphicon glyphicon-chevron-down"></span>
                </button>
            </h4>
        </div>
        <div id="customerManagementBody" class="panel-collapse collapse" aria-labelledby="customerManagementHeading">
        <div class="panel-body">
        <asp:UpdatePanel runat="server" ID="CustomerManagementUpdatePanel" UpdateMode="Always">
            <ContentTemplate>
            <div class="row">
                <div class="col-md-6">
                    <h4>Add New Customer</h4>
                    <p class="text-danger"><asp:Literal runat="server" ID="AddCustomerErrorMessage" /></p>
                    <p class="text-success"><asp:Literal runat="server" ID="AddCustomerSuccessMessage" Mode="Encode" /></p>
                    <div class="form-horizontal">
                        <div class="form-group">
                            <asp:Label runat="server" AssociatedControlID="NewCustomerUserName" CssClass="col-md-4 control-label">User name</asp:Label>
                            <div class="col-md-8">
                                <asp:TextBox runat="server" ID="NewCustomerUserName" CssClass="form-control" autocomplete="username" />
                            </div>
                        </div>
                        <div class="form-group">
                            <asp:Label runat="server" AssociatedControlID="NewCustomerPassword" CssClass="col-md-4 control-label">Password</asp:Label>
                            <div class="col-md-8">
                                <asp:TextBox runat="server" ID="NewCustomerPassword" TextMode="Password" CssClass="form-control" autocomplete="new-password" />
                            </div>
                        </div>
                        <div class="form-group">
                            <asp:Label runat="server" AssociatedControlID="NewCustomerConfirmPassword" CssClass="col-md-4 control-label">Confirm password</asp:Label>
                            <div class="col-md-8">
                                <asp:TextBox runat="server" ID="NewCustomerConfirmPassword" TextMode="Password" CssClass="form-control" autocomplete="new-password" />
                            </div>
                        </div>
                        <div class="form-group">
                            <div class="col-md-offset-4 col-md-8">
                                <asp:Button runat="server" OnClick="AddCustomer_Click" Text="Add Customer" CssClass="btn btn-primary" />
                            </div>
                        </div>
                    </div>
                </div>

                <div class="col-md-6">
                    <h4>Edit Customer</h4>
                    <p class="text-danger"><asp:Literal runat="server" ID="EditCustomerErrorMessage" /></p>
                    <p class="text-success"><asp:Literal runat="server" ID="EditCustomerSuccessMessage" Mode="Encode" /></p>
                    <div class="form-horizontal">
                        <div class="form-group">
                            <asp:Label runat="server" AssociatedControlID="EditCustomerDropDown" CssClass="col-md-4 control-label">Customer</asp:Label>
                            <div class="col-md-8">
                                <asp:DropDownList runat="server" ID="EditCustomerDropDown" AutoPostBack="true" OnSelectedIndexChanged="EditCustomerDropDown_SelectedIndexChanged" CssClass="form-control" />
                            </div>
                        </div>
                        <div class="form-group">
                            <asp:Label runat="server" AssociatedControlID="EditCustomerUserName" CssClass="col-md-4 control-label">User name</asp:Label>
                            <div class="col-md-8">
                                <asp:TextBox runat="server" ID="EditCustomerUserName" CssClass="form-control" autocomplete="username" />
                            </div>
                        </div>
                        <div class="form-group">
                            <asp:Label runat="server" AssociatedControlID="EditCustomerNewPassword" CssClass="col-md-4 control-label">New password</asp:Label>
                            <div class="col-md-8">
                                <asp:TextBox runat="server" ID="EditCustomerNewPassword" TextMode="Password" CssClass="form-control" autocomplete="new-password" />
                                <span class="help-block">Leave blank to keep the current password.</span>
                            </div>
                        </div>
                        <div class="form-group">
                            <asp:Label runat="server" AssociatedControlID="EditCustomerConfirmPassword" CssClass="col-md-4 control-label">Confirm new password</asp:Label>
                            <div class="col-md-8">
                                <asp:TextBox runat="server" ID="EditCustomerConfirmPassword" TextMode="Password" CssClass="form-control" autocomplete="new-password" />
                            </div>
                        </div>
                        <div class="form-group">
                            <div class="col-md-offset-4 col-md-8">
                                <asp:Button runat="server" OnClick="SaveCustomer_Click" Text="Save Changes" CssClass="btn btn-primary" />
                                <asp:Button runat="server" OnClick="DeleteCustomer_Click" Text="Delete Customer" CssClass="btn btn-danger"
                                    OnClientClick="return confirm('Are you sure? This customer will no longer be able to log in, but their existing tickets, comments, and history are kept.');" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            </ContentTemplate>
        </asp:UpdatePanel>
        </div>
        </div>
    </div>

    <div class="form-group">
        <asp:Label runat="server" AssociatedControlID="ViewEmployeeDropDown">Choose an employee to view</asp:Label>
        <asp:DropDownList runat="server" ID="ViewEmployeeDropDown" AutoPostBack="true" OnSelectedIndexChanged="ViewEmployeeDropDown_SelectedIndexChanged" CssClass="form-control" />
    </div>
    <asp:Panel runat="server" ID="ViewEmployeePanel" Visible="false" CssClass="panel panel-default">
        <div class="panel-heading"><asp:Literal runat="server" ID="ViewEmployeeNameLiteral" Mode="Encode" /></div>
        <div class="panel-body">
            <table class="table table-condensed table-fixed">
                <colgroup>
                    <col />
                    <col style="width : 120px" />
                </colgroup>
                <asp:Repeater runat="server" ID="ViewEmployeeTicketsRepeater">
                    <HeaderTemplate>
                        <thead><tr><th>Ticket Descriptions</th><th>Status</th></tr></thead>
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
            <asp:Label runat="server" ID="NoEmployeeTicketsLabel" Text="No tickets assigned." Visible="false" />
        </div>
    </asp:Panel>

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
