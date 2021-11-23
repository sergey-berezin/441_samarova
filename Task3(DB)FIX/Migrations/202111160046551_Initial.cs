namespace Task3_DB_FIX.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PictureInfoDB",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        image = c.Binary(),
                        rectangle = c.Binary(),
                        Type_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PictureTypeDB", t => t.Type_Id)
                .Index(t => t.Type_Id);
            
            CreateTable(
                "dbo.PictureTypeDB",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TypeName = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PictureInfoDB", "Type_Id", "dbo.PictureTypeDB");
            DropIndex("dbo.PictureInfoDB", new[] { "Type_Id" });
            DropTable("dbo.PictureTypeDB");
            DropTable("dbo.PictureInfoDB");
        }
    }
}
