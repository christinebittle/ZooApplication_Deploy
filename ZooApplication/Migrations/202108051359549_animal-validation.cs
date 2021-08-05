namespace ZooApplication.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class animalvalidation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Animals", "AnimalBio", c => c.String());
            AlterColumn("dbo.Animals", "AnimalName", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Animals", "AnimalName", c => c.String());
            DropColumn("dbo.Animals", "AnimalBio");
        }
    }
}
