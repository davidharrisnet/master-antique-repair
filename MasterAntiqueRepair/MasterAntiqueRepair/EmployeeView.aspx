<%@ Page Title="My Tickets" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="EmployeeView.aspx.cs" Inherits="EmployeeView" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <h2><%: Title %>.</h2>

    <h3>Unassigned Tickets</h3>
    <asp:GridView runat="server" ID="UnassignedGrid" AutoGenerateColumns="false" CssClass="table"
        OnRowCommand="UnassignedGrid_RowCommand">
        <Columns>
            <asp:BoundField DataField="Id" HeaderText="Id" />
            <asp:BoundField DataField="Description" HeaderText="Description" />
            <asp:BoundField DataField="SubmittedDate" HeaderText="Submitted" DataFormatString="{0:g}" />
            <asp:TemplateField>
                <ItemTemplate>
                    <asp:Button runat="server" Text="Assign to Me" CommandName="Take" CommandArgument='<%# Eval("Id") %>' CssClass="btn btn-success btn-sm" />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <EmptyDataTemplate>No unassigned tickets right now.</EmptyDataTemplate>
    </asp:GridView>

    <h3>My Tickets</h3>
    <asp:GridView runat="server" ID="MyTicketsGrid" AutoGenerateColumns="false" CssClass="table" OnRowDataBound="MyTicketsGrid_RowDataBound">
        <Columns>
            <asp:BoundField DataField="Id" HeaderText="Id" />
            <asp:BoundField DataField="Description" HeaderText="Description" />
            <asp:TemplateField HeaderText="Status">
                <ItemTemplate>
                    <span class='label <%# MasterAntiqueRepair.UiHelpers.StatusLabelClass((MasterAntiqueRepair.State.RepairState)Eval("State")) %>'><%# Eval("State") %></span>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="AssignedDate" HeaderText="Assigned" DataFormatString="{0:g}" />
            <asp:BoundField DataField="CompletedDate" HeaderText="Completed" DataFormatString="{0:g}" />
            <asp:TemplateField HeaderText="Employee Comments">
                <ItemTemplate>
                    <asp:Repeater runat="server" ID="EmployeeCommentsRepeater" OnItemCommand="EmployeeCommentsRepeater_ItemCommand">
                        <HeaderTemplate><ul></HeaderTemplate>
                        <ItemTemplate>
                            <li>
                                <span class="popover-toggle" tabindex="0" role="button" data-toggle="popover" data-trigger="focus" data-content='<%#: Eval("Text") %>'><%#: MasterAntiqueRepair.UiHelpers.Truncate(Eval("Text").ToString(), 60) %></span>
                                <small>(<%# Eval("CreatedAt", "{0:g}") %>)</small>
                                <br />
                                <button type="button" class="btn btn-link btn-xs"
                                    onclick="openEditCommentModal('<%# Eval("Id") %>', '<%# System.Web.HttpUtility.JavaScriptStringEncode(Eval("Text").ToString()) %>')">Edit</button>
                                <asp:LinkButton runat="server" CssClass="btn btn-link btn-xs text-danger" CommandName="DeleteComment" CommandArgument='<%# Eval("Id") %>'
                                    OnClientClick="return confirm('Are you sure?');">Delete</asp:LinkButton>
                            </li>
                        </ItemTemplate>
                        <FooterTemplate></ul></FooterTemplate>
                    </asp:Repeater>
                    <asp:Panel runat="server" Visible='<%# (MasterAntiqueRepair.State.RepairState)Eval("State") == MasterAntiqueRepair.State.RepairState.COMPLETED %>'>
                        <button type="button" class="btn btn-info btn-sm"
                            onclick="openAddCommentModal('<%# Eval("Id") %>')">
                            Add Comment
                        </button>
                    </asp:Panel>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Customer Comments">
                <ItemTemplate>
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
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Complete">
                <ItemTemplate>
                    <asp:Panel runat="server" Visible='<%# (MasterAntiqueRepair.State.RepairState)Eval("State") != MasterAntiqueRepair.State.RepairState.COMPLETED %>'>
                        <button type="button" class="btn btn-success btn-sm"
                            onclick="openCompleteModal('<%# Eval("Id") %>', '<%# System.Web.HttpUtility.JavaScriptStringEncode(Eval("Description").ToString()) %>')">
                            Mark Complete
                        </button>
                    </asp:Panel>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <EmptyDataTemplate>You haven't picked up any tickets yet.</EmptyDataTemplate>
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
                    <asp:HiddenField runat="server" ID="CompleteTicketId" />
                    <div class="form-group">
                        <asp:Label runat="server" AssociatedControlID="ModalCommentBox">Employee Comment</asp:Label>
                        <asp:TextBox runat="server" ID="ModalCommentBox" TextMode="MultiLine" Rows="3" CssClass="form-control" />
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-default" data-dismiss="modal">Cancel</button>
                    <asp:Button runat="server" ID="ConfirmCompleteButton" Text="Mark Complete" OnClick="ConfirmComplete_Click" CssClass="btn btn-success" />
                </div>
            </div>
        </div>
    </div>

    <div class="modal fade" id="addCommentModal" tabindex="-1" role="dialog" aria-labelledby="addCommentModalLabel">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                    <h4 class="modal-title" id="addCommentModalLabel">Add Comment</h4>
                </div>
                <div class="modal-body">
                    <asp:HiddenField runat="server" ID="AddCommentTicketId" />
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

    <div class="modal fade" id="editCommentModal" tabindex="-1" role="dialog" aria-labelledby="editCommentModalLabel">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                    <h4 class="modal-title" id="editCommentModalLabel">Edit Comment</h4>
                </div>
                <div class="modal-body">
                    <asp:HiddenField runat="server" ID="EditCommentId" />
                    <asp:Label runat="server" ID="EditCommentErrorLabel" CssClass="text-danger" Visible="false" />
                    <div class="form-group">
                        <asp:Label runat="server" AssociatedControlID="EditCommentText">Comment</asp:Label>
                        <asp:TextBox runat="server" ID="EditCommentText" TextMode="MultiLine" Rows="3" CssClass="form-control" />
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-default" data-dismiss="modal">Cancel</button>
                    <asp:Button runat="server" ID="SaveEditCommentButton" Text="Save" OnClick="SaveEditComment_Click" CssClass="btn btn-primary" />
                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        function openCompleteModal(ticketId, description) {
            document.getElementById('<%= CompleteTicketId.ClientID %>').value = ticketId;
            document.getElementById('completeModalDescription').innerText = description;
            $('#completeModal').modal('show');
        }

        function openAddCommentModal(ticketId) {
            document.getElementById('<%= AddCommentTicketId.ClientID %>').value = ticketId;
            $('#addCommentModal').modal('show');
        }

        function openEditCommentModal(commentId, currentText) {
            document.getElementById('<%= EditCommentId.ClientID %>').value = commentId;
            document.getElementById('<%= EditCommentText.ClientID %>').value = currentText;
            $('#editCommentModal').modal('show');
        }

        $(function () {
            $('[data-toggle="popover"]').popover();
        });
    </script>
</asp:Content>
