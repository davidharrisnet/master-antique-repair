namespace MasterAntiqueRepairData.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SyncWithSharedModel : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Orders", "User_Id", "dbo.Users");
            DropForeignKey("dbo.Users", "UserRole_Id", "dbo.UserRoles");
            DropIndex("dbo.Orders", new[] { "User_Id" });
            DropIndex("dbo.Users", new[] { "UserRole_Id" });
            CreateTable(
                "dbo.Tickets",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        State = c.Int(nullable: false),
                        Description = c.String(maxLength: 2000),
                        SubmittedDate = c.DateTime(),
                        AssignedDate = c.DateTime(),
                        CompletedDate = c.DateTime(),
                        User_Id = c.Int(),
                        Customer_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .ForeignKey("dbo.Users", t => t.Customer_Id)
                .Index(t => t.User_Id)
                .Index(t => t.Customer_Id);
            
            CreateTable(
                "dbo.Comments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        TicketId = c.Int(nullable: false),
                        Text = c.String(maxLength: 2000),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Tickets", t => t.TicketId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.TicketId);
            
            AddColumn("dbo.Users", "PasswordHash", c => c.String());
            AddColumn("dbo.Users", "FailedLoginAttempts", c => c.Int(nullable: false));
            AddColumn("dbo.Users", "LockedOutUntil", c => c.DateTime());
            DropColumn("dbo.Users", "UserRole_Id");
            DropTable("dbo.Orders");
            DropTable("dbo.UserRoles");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.UserRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserRoleValue = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Orders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        State = c.Int(nullable: false),
                        Description = c.String(),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Users", "UserRole_Id", c => c.Int());
            DropForeignKey("dbo.Tickets", "Customer_Id", "dbo.Users");
            DropForeignKey("dbo.Comments", "UserId", "dbo.Users");
            DropForeignKey("dbo.Tickets", "User_Id", "dbo.Users");
            DropForeignKey("dbo.Comments", "TicketId", "dbo.Tickets");
            DropIndex("dbo.Comments", new[] { "TicketId" });
            DropIndex("dbo.Comments", new[] { "UserId" });
            DropIndex("dbo.Tickets", new[] { "Customer_Id" });
            DropIndex("dbo.Tickets", new[] { "User_Id" });
            DropColumn("dbo.Users", "LockedOutUntil");
            DropColumn("dbo.Users", "FailedLoginAttempts");
            DropColumn("dbo.Users", "PasswordHash");
            DropTable("dbo.Comments");
            DropTable("dbo.Tickets");
            CreateIndex("dbo.Users", "UserRole_Id");
            CreateIndex("dbo.Orders", "User_Id");
            AddForeignKey("dbo.Users", "UserRole_Id", "dbo.UserRoles", "Id");
            AddForeignKey("dbo.Orders", "User_Id", "dbo.Users", "Id");
        }
    }
}
