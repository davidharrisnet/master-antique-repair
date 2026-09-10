namespace MasterAntiqueRepairData.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserRole : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserRoleValue = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Users", "UserRole_Id", c => c.Int());
            CreateIndex("dbo.Users", "UserRole_Id");
            AddForeignKey("dbo.Users", "UserRole_Id", "dbo.UserRoles", "Id");
            DropColumn("dbo.Users", "Role");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Users", "Role", c => c.Int(nullable: false));
            DropForeignKey("dbo.Users", "UserRole_Id", "dbo.UserRoles");
            DropIndex("dbo.Users", new[] { "UserRole_Id" });
            DropColumn("dbo.Users", "UserRole_Id");
            DropTable("dbo.UserRoles");
        }
    }
}
