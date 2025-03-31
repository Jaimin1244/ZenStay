namespace Learn_Auth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreated : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Bookings",
                c => new
                    {
                        BookingID = c.Int(nullable: false, identity: true),
                        UserID = c.Int(nullable: false),
                        RoomID = c.Int(nullable: false),
                        BookingDate = c.DateTime(nullable: false),
                        CheckInDate = c.DateTime(nullable: false),
                        CheckOutDate = c.DateTime(nullable: false),
                        BookingStatus = c.String(),
                    })
                .PrimaryKey(t => t.BookingID)
                .ForeignKey("dbo.Rooms", t => t.RoomID, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserID, cascadeDelete: true)
                .Index(t => t.UserID)
                .Index(t => t.RoomID);
            
            CreateTable(
                "dbo.Invoices",
                c => new
                    {
                        InvoiceID = c.Int(nullable: false, identity: true),
                        BookingID = c.Int(nullable: false),
                        CustomerName = c.String(),
                        CustomerAddress = c.String(),
                        HotelName = c.String(),
                        RoomNo = c.String(),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        InvoiceDate = c.DateTime(nullable: false),
                        TotalAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Tax = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Discounts = c.Decimal(nullable: false, precision: 18, scale: 2),
                        FinalAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.InvoiceID)
                .ForeignKey("dbo.Bookings", t => t.BookingID, cascadeDelete: true)
                .Index(t => t.BookingID);
            
            CreateTable(
                "dbo.Payments",
                c => new
                    {
                        PaymentID = c.Int(nullable: false, identity: true),
                        BookingID = c.Int(nullable: false),
                        PaymentDate = c.DateTime(nullable: false),
                        PaymentAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PaymentMethod = c.String(),
                        PaymentStatus = c.String(),
                        RefundStatus = c.String(),
                    })
                .PrimaryKey(t => t.PaymentID)
                .ForeignKey("dbo.Bookings", t => t.BookingID, cascadeDelete: true)
                .Index(t => t.BookingID);
            
            CreateTable(
                "dbo.Rooms",
                c => new
                    {
                        RoomId = c.Int(nullable: false, identity: true),
                        HotelId = c.Int(nullable: false),
                        RoomNumber = c.String(),
                        RoomType = c.String(),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IsAvailable = c.Boolean(nullable: false),
                        Amenities = c.String(),
                        RoomImage = c.String(),
                    })
                .PrimaryKey(t => t.RoomId)
                .ForeignKey("dbo.Hotels", t => t.HotelId, cascadeDelete: true)
                .Index(t => t.HotelId);
            
            CreateTable(
                "dbo.Hotels",
                c => new
                    {
                        HotelId = c.Int(nullable: false, identity: true),
                        HotelName = c.String(),
                        Address = c.String(),
                        City = c.String(),
                        State = c.String(),
                        Country = c.String(),
                        ZipCode = c.String(),
                        Description = c.String(),
                        ContactInfo = c.String(),
                        HotelImages = c.String(),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.HotelId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserName = c.String(nullable: false),
                        Email = c.String(nullable: false),
                        PasswordHash = c.String(nullable: false),
                        RoleId = c.Int(nullable: false),
                        FirstName = c.String(),
                        LastName = c.String(),
                        ContactNo = c.String(),
                        ProfilePicture = c.String(),
                        Address = c.String(),
                        OTP = c.String(),
                        OTPExpiry = c.DateTime(),
                        IsVerified = c.Boolean(nullable: false),
                        ResetToken = c.String(),
                        ResetTokenExpiry = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Roles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Roles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Bookings", "UserID", "dbo.Users");
            DropForeignKey("dbo.Bookings", "RoomID", "dbo.Rooms");
            DropForeignKey("dbo.Rooms", "HotelId", "dbo.Hotels");
            DropForeignKey("dbo.Hotels", "UserId", "dbo.Users");
            DropForeignKey("dbo.Users", "RoleId", "dbo.Roles");
            DropForeignKey("dbo.Payments", "BookingID", "dbo.Bookings");
            DropForeignKey("dbo.Invoices", "BookingID", "dbo.Bookings");
            DropIndex("dbo.Users", new[] { "RoleId" });
            DropIndex("dbo.Hotels", new[] { "UserId" });
            DropIndex("dbo.Rooms", new[] { "HotelId" });
            DropIndex("dbo.Payments", new[] { "BookingID" });
            DropIndex("dbo.Invoices", new[] { "BookingID" });
            DropIndex("dbo.Bookings", new[] { "RoomID" });
            DropIndex("dbo.Bookings", new[] { "UserID" });
            DropTable("dbo.Roles");
            DropTable("dbo.Users");
            DropTable("dbo.Hotels");
            DropTable("dbo.Rooms");
            DropTable("dbo.Payments");
            DropTable("dbo.Invoices");
            DropTable("dbo.Bookings");
        }
    }
}
