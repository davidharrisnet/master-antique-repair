<%@ Page Title="My Repairs" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="CustomerView.aspx.cs" Inherits="CustomerView" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <h2><%: Title %>.</h2>

    <p><a runat="server" href="~/SubmitRepair" class="btn btn-default">Submit a new repair request</a></p>

    <h3>My Repair Requests</h3>
    <asp:GridView runat="server" ID="MyTicketsGrid" AutoGenerateColumns="false" CssClass="table" OnRowDataBound="MyTicketsGrid_RowDataBound">
        <Columns>
            <asp:BoundField DataField="Id" HeaderText="Id" />
            <asp:BoundField DataField="Description" HeaderText="Description" />
            <asp:BoundField DataField="State" HeaderText="Status" />
            <asp:BoundField DataField="SubmittedDate" HeaderText="Submitted" DataFormatString="{0:g}" />
            <asp:TemplateField HeaderText="Employee Comments">
                <ItemTemplate>
                    <asp:Repeater runat="server" ID="EmployeeCommentsRepeater">
                        <HeaderTemplate><ul></HeaderTemplate>
                        <ItemTemplate>
                            <li><%# Eval("Text") %> <small>(<%# Eval("CreatedAt", "{0:g}") %>)</small></li>
                        </ItemTemplate>
                        <FooterTemplate></ul></FooterTemplate>
                    </asp:Repeater>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Customer Comments">
                <ItemTemplate>
                    <asp:Repeater runat="server" ID="CommentsRepeater">
                        <HeaderTemplate><ul></HeaderTemplate>
                        <ItemTemplate>
                            <li><%# Eval("Text") %> <small>(<%# Eval("CreatedAt", "{0:g}") %>)</small></li>
                        </ItemTemplate>
                        <FooterTemplate></ul></FooterTemplate>
                    </asp:Repeater>
                    <asp:Panel runat="server" Visible='<%# (MasterAntiqueRepair.State.RepairState)Eval("State") == MasterAntiqueRepair.State.RepairState.COMPLETED %>'>
                        <button type="button" class="btn btn-default btn-sm"
                            onclick="openAddCommentModal('<%# Eval("Id") %>')">
                            Add Comment
                        </button>
                    </asp:Panel>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <EmptyDataTemplate>You haven't submitted any repair requests yet.</EmptyDataTemplate>
    </asp:GridView>

    <div class="modal fade" id="addCommentModal" tabindex="-1" role="dialog" aria-labelledby="addCommentModalLabel">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                    <h4 class="modal-title" id="addCommentModalLabel">Add Comment</h4>
                </div>
                <div class="modal-body">
                    <asp:HiddenField runat="server" ID="CommentTicketId" />
                    <asp:Label runat="server" ID="CommentErrorLabel" CssClass="text-danger" Visible="false" />
                    <div class="form-group">
                        <asp:Label runat="server" AssociatedControlID="NewCommentText">Comment</asp:Label>
                        <asp:TextBox runat="server" ID="NewCommentText" TextMode="MultiLine" Rows="3" CssClass="form-control" />
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-default" data-dismiss="modal">Cancel</button>
                    <asp:Button runat="server" ID="PostCommentButton" Text="Post Comment" OnClick="PostComment_Click" CssClass="btn btn-primary" />
                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        function openAddCommentModal(ticketId) {
            document.getElementById('<%= CommentTicketId.ClientID %>').value = ticketId;
            $('#addCommentModal').modal('show');
        }
    </script>
</asp:Content>
