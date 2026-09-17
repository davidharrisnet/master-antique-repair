namespace MasterAntiqueRepairData.Migrations.RepairShop
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAspNetIdentity : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.PasswordResetTokens", "UserId", "dbo.Users");
            DropIndex("dbo.PasswordResetTokens", new[] { "UserId" });
            CreateTable(
                "dbo.UserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.UserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.UserRoles",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.Roles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Roles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            AddColumn("dbo.Users", "Email", c => c.String(maxLength: 256));
            AddColumn("dbo.Users", "EmailConfirmed", c => c.Boolean(nullable: false));
            AddColumn("dbo.Users", "SecurityStamp", c => c.String());
            AddColumn("dbo.Users", "PhoneNumber", c => c.String());
            AddColumn("dbo.Users", "PhoneNumberConfirmed", c => c.Boolean(nullable: false));
            AddColumn("dbo.Users", "TwoFactorEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.Users", "LockoutEndDateUtc", c => c.DateTime());
            AddColumn("dbo.Users", "LockoutEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.Users", "AccessFailedCount", c => c.Int(nullable: false));

            // IX_Users_Name_Active (a filtered index - see AddUniqueActiveUsername) isn't
            // visible to EF's Code First model, so the scaffolder doesn't know it depends on
            // Name and blocks the AlterColumn below. Drop it first, alter the column, then
            // recreate it. Deliberately NOT creating Identity's own UserNameIndex (an
            // unconditional unique index) here - it would defeat the soft-delete
            // username-reuse rule IX_Users_Name_Active exists to enforce, by blocking reuse
            // of a soft-deleted user's old username.
            Sql("DROP INDEX IX_Users_Name_Active ON dbo.Users;");
            AlterColumn("dbo.Users", "Name", c => c.String(nullable: false, maxLength: 256));
            Sql("CREATE UNIQUE INDEX IX_Users_Name_Active ON dbo.Users (Name) WHERE DeletedAt IS NULL;");

            DropColumn("dbo.Users", "FailedLoginAttempts");
            DropColumn("dbo.Users", "LockedOutUntil");
            DropTable("dbo.PasswordResetTokens");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.PasswordResetTokens",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        Token = c.String(maxLength: 200),
                        CreatedAt = c.DateTime(nullable: false),
                        ExpiresAt = c.DateTime(nullable: false),
                        UsedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Users", "LockedOutUntil", c => c.DateTime());
            AddColumn("dbo.Users", "FailedLoginAttempts", c => c.Int(nullable: false));
            DropForeignKey("dbo.UserRoles", "RoleId", "dbo.Roles");
            DropForeignKey("dbo.UserRoles", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserLogins", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserClaims", "UserId", "dbo.Users");
            DropIndex("dbo.Roles", "RoleNameIndex");
            DropIndex("dbo.UserRoles", new[] { "RoleId" });
            DropIndex("dbo.UserRoles", new[] { "UserId" });
            DropIndex("dbo.UserLogins", new[] { "UserId" });
            DropIndex("dbo.UserClaims", new[] { "UserId" });
            Sql("DROP INDEX IX_Users_Name_Active ON dbo.Users;");
            AlterColumn("dbo.Users", "Name", c => c.String(maxLength: 256));
            Sql("CREATE UNIQUE INDEX IX_Users_Name_Active ON dbo.Users (Name) WHERE DeletedAt IS NULL;");
            DropColumn("dbo.Users", "AccessFailedCount");
            DropColumn("dbo.Users", "LockoutEnabled");
            DropColumn("dbo.Users", "LockoutEndDateUtc");
            DropColumn("dbo.Users", "TwoFactorEnabled");
            DropColumn("dbo.Users", "PhoneNumberConfirmed");
            DropColumn("dbo.Users", "PhoneNumber");
            DropColumn("dbo.Users", "SecurityStamp");
            DropColumn("dbo.Users", "EmailConfirmed");
            DropColumn("dbo.Users", "Email");
            DropTable("dbo.Roles");
            DropTable("dbo.UserRoles");
            DropTable("dbo.UserLogins");
            DropTable("dbo.UserClaims");
            CreateIndex("dbo.PasswordResetTokens", "UserId");
            AddForeignKey("dbo.PasswordResetTokens", "UserId", "dbo.Users", "Id", cascadeDelete: true);
        }
    }
}
