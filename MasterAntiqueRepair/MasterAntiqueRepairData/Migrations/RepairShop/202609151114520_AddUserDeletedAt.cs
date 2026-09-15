namespace MasterAntiqueRepairData.Migrations.RepairShop
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserDeletedAt : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "DeletedAt", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "DeletedAt");
        }
    }
}
