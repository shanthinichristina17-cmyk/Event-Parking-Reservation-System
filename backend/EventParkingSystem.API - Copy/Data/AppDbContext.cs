using EventParkingSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EventParkingSystem.API.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<EventCategory> EventCategories => Set<EventCategory>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<ParkingSlot> ParkingSlots => Set<ParkingSlot>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingSeat> BookingSeats => Set<BookingSeat>();
    public DbSet<ParkingReservation> ParkingReservations => Set<ParkingReservation>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(e =>
        {
            e.HasKey(x => x.CustomerId);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(30);
            e.Property(x => x.PasswordHash).HasMaxLength(200).IsRequired();
            e.Property(x => x.Role).HasMaxLength(20).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<Venue>(e =>
        {
            e.HasKey(x => x.VenueId);
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Address).HasMaxLength(300).IsRequired();
        });

        modelBuilder.Entity<EventCategory>(e =>
        {
            e.HasKey(x => x.CategoryId);
            e.HasIndex(x => x.Name).IsUnique();
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Event>(e =>
        {
            e.HasKey(x => x.EventId);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.TicketPrice).HasColumnType("decimal(10,2)");
            e.Property(x => x.ParkingFee).HasColumnType("decimal(10,2)");
            e.HasIndex(x => x.EventDate);
            e.HasIndex(x => x.VenueId);
            e.HasIndex(x => x.CategoryId);
            e.HasOne(x => x.Venue).WithMany(x => x.Events).HasForeignKey(x => x.VenueId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Category).WithMany(x => x.Events).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Seat>(e =>
        {
            e.HasKey(x => x.SeatId);
            e.Property(x => x.SeatRow).HasMaxLength(10).IsRequired();
            e.Property(x => x.SeatNumber).HasMaxLength(20).IsRequired();
            e.Property(x => x.SeatType).HasMaxLength(50);
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.Price).HasColumnType("decimal(10,2)");
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasIndex(x => new { x.EventId, x.SeatRow, x.SeatNumber }).IsUnique();
            e.HasIndex(x => new { x.EventId, x.Status });
            e.HasOne(x => x.Event).WithMany(x => x.Seats).HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ParkingSlot>(e =>
        {
            e.HasKey(x => x.SlotId);
            e.Property(x => x.Zone).HasMaxLength(30);
            e.Property(x => x.SlotNumber).HasMaxLength(30).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.Fee).HasColumnType("decimal(10,2)");
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasIndex(x => new { x.EventId, x.SlotNumber }).IsUnique();
            e.HasIndex(x => new { x.EventId, x.Status });
            e.HasOne(x => x.Event).WithMany(x => x.ParkingSlots).HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Booking>(e =>
        {
            e.HasKey(x => x.BookingId);
            e.HasIndex(x => x.BookingNumber).IsUnique();
            e.Property(x => x.BookingNumber).HasMaxLength(40).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.TotalAmount).HasColumnType("decimal(10,2)");
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasIndex(x => x.CustomerId);
            e.HasIndex(x => x.EventId);
            e.HasIndex(x => new { x.Status, x.HoldExpiresAt });
            e.HasOne(x => x.Customer).WithMany(x => x.Bookings).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Event).WithMany(x => x.Bookings).HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BookingSeat>(e =>
        {
            e.HasKey(x => x.BookingSeatId);
            e.Property(x => x.PriceAtBooking).HasColumnType("decimal(10,2)");
            e.HasIndex(x => new { x.BookingId, x.SeatId }).IsUnique();
            e.HasIndex(x => x.SeatId).IsUnique().HasFilter("[IsActive] = 1");
            e.HasOne(x => x.Booking).WithMany(x => x.BookingSeats).HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Seat).WithMany().HasForeignKey(x => x.SeatId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ParkingReservation>(e =>
        {
            e.HasKey(x => x.ReservationId);
            e.Property(x => x.FeeAtReservation).HasColumnType("decimal(10,2)");
            e.HasIndex(x => x.BookingId).IsUnique();
            e.HasIndex(x => x.SlotId).IsUnique().HasFilter("[IsActive] = 1");
            e.HasOne(x => x.Booking).WithOne(x => x.ParkingReservation).HasForeignKey<ParkingReservation>(x => x.BookingId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Slot).WithMany().HasForeignKey(x => x.SlotId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Payment>(e =>
        {
            e.HasKey(x => x.PaymentId);
            e.Property(x => x.Amount).HasColumnType("decimal(10,2)");
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.ReceiptNumber).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.BookingId).IsUnique();
            e.HasIndex(x => x.ReceiptNumber).IsUnique();
            e.HasOne(x => x.Booking).WithOne(x => x.Payment).HasForeignKey<Payment>(x => x.BookingId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Notification>(e =>
        {
            e.HasKey(x => x.NotificationId);
            e.Property(x => x.Type).HasMaxLength(30).IsRequired();
            e.Property(x => x.Message).HasMaxLength(600).IsRequired();
            e.HasIndex(x => new { x.CustomerId, x.CreatedAt });
            e.HasOne(x => x.Customer).WithMany(x => x.Notifications).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
