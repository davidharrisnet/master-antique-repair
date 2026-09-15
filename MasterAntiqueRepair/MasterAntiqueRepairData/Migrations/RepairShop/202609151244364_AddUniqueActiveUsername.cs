namespace MasterAntiqueRepairData.Migrations.RepairShop
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUniqueActiveUsername : DbMigration
    {
        public override void Up()
        {
            // Name was created as nvarchar(max) (InitialCreate's plain c.String()), which SQL
            // Server refuses to use as an index key column -- must narrow it first. 256 is
            // generous for a login name and doesn't touch the C# model/DataAnnotations, so it
            // doesn't affect EF6's model hash used by the "pending model changes" check;
            // nullability unchanged (still NULL) to match the original column exactly.
            Sql("ALTER TABLE dbo.Users ALTER COLUMN Name nvarchar(256) NULL;");

            // Filtered, not a plain unique constraint - only active (DeletedAt IS NULL)
            // rows are checked, so a soft-deleted user's old username stays reusable by a
            // new account, matching the app-level rule already enforced in code. Closes
            // the check-then-write race that let two active "customer3" rows get created
            // (see claude.log entry 75) - the database itself now rejects the second
            // INSERT/UPDATE outright, rather than relying solely on the .Any() check that
            // two near-simultaneous requests could both pass before either commits.
            Sql("CREATE UNIQUE INDEX IX_Users_Name_Active ON dbo.Users (Name) WHERE DeletedAt IS NULL;");
        }

        public override void Down()
        {
            Sql("DROP INDEX IX_Users_Name_Active ON dbo.Users;");
            Sql("ALTER TABLE dbo.Users ALTER COLUMN Name nvarchar(max) NULL;");
        }
    }
}
