namespace MasterAntiqueRepairData.Migrations.RepairShop
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLoginLockout : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "FailedLoginAttempts", c => c.Int(nullable: false));
            AddColumn("dbo.Users", "LockedOutUntil", c => c.DateTime());
            AlterColumn("dbo.Tickets", "Description", c => c.String(maxLength: 2000));
            AlterColumn("dbo.Comments", "Text", c => c.String(maxLength: 2000));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Comments", "Text", c => c.String());
            AlterColumn("dbo.Tickets", "Description", c => c.String());
            DropColumn("dbo.Users", "LockedOutUntil");
            DropColumn("dbo.Users", "FailedLoginAttempts");
        }
    }
}
