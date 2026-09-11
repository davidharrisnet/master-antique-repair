namespace MasterAntiqueRepairData.Migrations.RepairShop
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            // This migration ID is already recorded as applied in __MigrationHistory for
            // the live dev database (Users/Orders were originally hand-built there via
            // manual ALTER TABLE statements before this context was put under Migrations),
            // so EF6 tracks migrations applied by ID, not by content - rewriting this body
            // does not make it re-run there. It only matters the other way: on a genuinely
            // empty database, RepairShopContext defaults to MigrateDatabaseToLatestVersion
            // (automatic, since a Migrations Configuration exists for it) rather than
            // CreateDatabaseIfNotExists, so migration replay - starting with this one - is
            // the only thing that builds the schema. It has to actually create the tables.
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        PasswordHash = c.String(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
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
                        Customer_Id = c.Int(),
                        Comment = c.String(),
                        SubmittedDate = c.DateTime(),
                        AssignedDate = c.DateTime(),
                        CompletedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .ForeignKey("dbo.Users", t => t.Customer_Id)
                .Index(t => t.User_Id)
                .Index(t => t.Customer_Id);
        }

        public override void Down()
        {
            DropForeignKey("dbo.Orders", "Customer_Id", "dbo.Users");
            DropForeignKey("dbo.Orders", "User_Id", "dbo.Users");
            DropIndex("dbo.Orders", new[] { "Customer_Id" });
            DropIndex("dbo.Orders", new[] { "User_Id" });
            DropTable("dbo.Orders");
            DropTable("dbo.Users");
        }
    }
}
