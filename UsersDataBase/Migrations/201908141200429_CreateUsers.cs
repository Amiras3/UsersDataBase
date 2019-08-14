namespace UsersDataBase.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateUsers : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Robots",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        RobotName = c.String(),
                        User_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Users", t => t.User_ID)
                .Index(t => t.User_ID);
            
            CreateTable(
                "dbo.Sessions",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        SessionName = c.String(),
                        Robot_ID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Robots", t => t.Robot_ID, cascadeDelete: true)
                .Index(t => t.Robot_ID);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        UserName = c.Int(nullable: false),
                        Email = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Robots", "User_ID", "dbo.Users");
            DropForeignKey("dbo.Sessions", "Robot_ID", "dbo.Robots");
            DropIndex("dbo.Sessions", new[] { "Robot_ID" });
            DropIndex("dbo.Robots", new[] { "User_ID" });
            DropTable("dbo.Users");
            DropTable("dbo.Sessions");
            DropTable("dbo.Robots");
        }
    }
}
