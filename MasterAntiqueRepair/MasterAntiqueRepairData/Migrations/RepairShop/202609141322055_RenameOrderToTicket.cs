namespace MasterAntiqueRepairData.Migrations.RepairShop
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameOrderToTicket : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Orders", newName: "Tickets");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.Tickets", newName: "Orders");
        }
    }
}
