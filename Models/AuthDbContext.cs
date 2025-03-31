using System.Data.Entity;
using Learn_Auth.Models;

namespace Learn_Auth.Models
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext() : base("DefaultConnection") 
        {
            this.Configuration.LazyLoadingEnabled = true;
            this.Configuration.ProxyCreationEnabled = true;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Invoice> Invoices { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuring relationships

            // User - Role (Many-to-One)
            modelBuilder.Entity<User>()
                .HasRequired(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);

            // User - Hotel (One-to-Many)
            modelBuilder.Entity<Hotel>()
                .HasRequired(h => h.User)
                .WithMany(u => u.Hotels)
                .HasForeignKey(h => h.UserId)
                .WillCascadeOnDelete(false); 

            // Room - Hotel (Many-to-One)
            modelBuilder.Entity<Room>()
                .HasRequired(r => r.Hotel)
                .WithMany(h => h.Rooms)
                .HasForeignKey(r => r.HotelId);

            // Booking - User (Many-to-One)
            modelBuilder.Entity<Booking>()
                .HasRequired(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserID);

            // Booking - Room (Many-to-One)
            modelBuilder.Entity<Booking>()
                .HasRequired(b => b.Room)
                .WithMany(r => r.Bookings)
                .HasForeignKey(b => b.RoomID);

            // Payment - Booking (Many-to-One)
            modelBuilder.Entity<Payment>()
                .HasRequired(p => p.Booking)
                .WithMany(b => b.Payments)
                .HasForeignKey(p => p.BookingID);

            // Invoice - Booking (Many-to-One)
            modelBuilder.Entity<Invoice>()
                .HasRequired(i => i.Booking)
                .WithMany(b => b.Invoices)
                .HasForeignKey(i => i.BookingID);
        }

    }
    }