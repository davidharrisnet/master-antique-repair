<%@ Page Title="My Jobs" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="EmployeeView.aspx.cs" Inherits="EmployeeView" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <h2><%: Title %>.</h2>

    <h3>Unassigned Jobs</h3>
    <asp:GridView runat="server" ID="UnassignedGrid" AutoGenerateColumns="false" CssClass="table"
        OnRowCommand="UnassignedGrid_RowCommand">
        <Columns>
            <asp:BoundField DataField="Id" HeaderText="Id" />
            <asp:BoundField DataField="Description" HeaderText="Description" />
            <asp:BoundField DataField="SubmittedDate" HeaderText="Submitted" DataFormatString="{0:g}" />
            <asp:TemplateField>
                <ItemTemplate>
                    <asp:Button runat="server" Text="Assign to Me" CommandName="Take" CommandArgument='<%# Eval("Id") %>' CssClass="btn btn-default btn-sm" />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <EmptyDataTemplate>No unassigned jobs right now.</EmptyDataTemplate>
    </asp:GridView>

    <h3>My Jobs</h3>
    <asp:GridView runat="server" ID="MyJobsGrid" AutoGenerateColumns="false" CssClass="table">
        <Columns>
            <asp:BoundField DataField="Id" HeaderText="Id" />
            <asp:BoundField DataField="Description" HeaderText="Description" />
            <asp:BoundField DataField="State" HeaderText="Status" />
            <asp:BoundField DataField="AssignedDate" HeaderText="Assigned" DataFormatString="{0:g}" />
            <asp:BoundField DataField="CompletedDate" HeaderText="Completed" DataFormatString="{0:g}" />
            <asp:BoundField DataField="Comment" HeaderText="Comment" />
            <asp:TemplateField HeaderText="Complete">
                <ItemTemplate>
                    <asp:Panel runat="server" Visible='<%# (MasterAntiqueRepair.State.RepairState)Eval("State") != MasterAntiqueRepair.State.RepairState.COMPLETED %>'>
                        <button type="button" class="btn btn-default btn-sm"
                            onclick="openCompleteModal('<%# Eval("Id") %>', '<%# System.Web.HttpUtility.JavaScriptStringEncode(Eval("Description").ToString()) %>')">
                            Mark Complete
                        </button>
                    </asp:Panel>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <EmptyDataTemplate>You haven't picked up any jobs yet.</EmptyDataTemplate>
    </asp:GridView>

    <div class="modal fade" id="completeModal" tabindex="-1" role="dialog" aria-labelledby="completeModalLabel">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                    <h4 class="modal-title" id="completeModalLabel">Complete Repair</h4>
                </div>
                <div class="modal-body">
                    <p id="completeModalDescription"></p>
                    <asp:HiddenField runat="server" ID="CompleteOrderId" />
                    <div class="form-group">
                        <asp:Label runat="server" AssociatedControlID="ModalCommentBox">Comment</asp:Label>
                        <asp:TextBox runat="server" ID="ModalCommentBox" TextMode="MultiLine" Rows="3" CssClass="form-control" />
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-default" data-dismiss="modal">Cancel</button>
                    <asp:Button runat="server" ID="ConfirmCompleteButton" Text="Mark Complete" OnClick="ConfirmComplete_Click" CssClass="btn btn-primary" />
                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        function openCompleteModal(orderId, description) {
            document.getElementById('<%= CompleteOrderId.ClientID %>').value = orderId;
            document.getElementById('completeModalDescription').innerText = description;
            $('#completeModal').modal('show');
        }
    </script>
</asp:Content>
