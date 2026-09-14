namespace MasterAntiqueRepairData.Migrations.RepairShop
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveTicketComment : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Tickets", "Comment");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Tickets", "Comment", c => c.String());
        }
    }
}
