namespace MasterAntiqueRepairData.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCustomer : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Orders", "State_Id", "dbo.States");
            DropIndex("dbo.Orders", new[] { "State_Id" });
            AddColumn("dbo.Orders", "State", c => c.Int(nullable: false));
            AddColumn("dbo.Users", "Discriminator", c => c.String(nullable: false, maxLength: 128));
            DropColumn("dbo.Orders", "State_Id");
            DropTable("dbo.States");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.States",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RepairStateValue = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Orders", "State_Id", c => c.Int());
            DropColumn("dbo.Users", "Discriminator");
            DropColumn("dbo.Orders", "State");
            CreateIndex("dbo.Orders", "State_Id");
            AddForeignKey("dbo.Orders", "State_Id", "dbo.States", "Id");
        }
    }
}
