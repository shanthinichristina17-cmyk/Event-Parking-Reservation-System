namespace EventParkingSystem.API.Models;

public class Customer
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Customer";
    public string Status { get; set; } = "Active";
    public bool EmailVerified { get; set; }
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiresAt { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}

public class Venue
{
    public int VenueId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Event> Events { get; set; } = new List<Event>();
}

public class EventCategory
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Event> Events { get; set; } = new List<Event>();
}

public class Event
{
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int VenueId { get; set; }
    public int CategoryId { get; set; }
    public DateOnly EventDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public decimal TicketPrice { get; set; }
    public decimal ParkingFee { get; set; }
    public int Capacity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Venue? Venue { get; set; }
    public EventCategory? Category { get; set; }
    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    public ICollection<ParkingSlot> ParkingSlots { get; set; } = new List<ParkingSlot>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

public class Seat
{
    public int SeatId { get; set; }
    public int EventId { get; set; }
    public string SeatRow { get; set; } = string.Empty;
    public string SeatNumber { get; set; } = string.Empty;
    public string? SeatType { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; } = "Available";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public Event? Event { get; set; }
}

public class ParkingSlot
{
    public int SlotId { get; set; }
    public int EventId { get; set; }
    public string? Zone { get; set; }
    public string SlotNumber { get; set; } = string.Empty;
    public decimal Fee { get; set; }
    public string Status { get; set; } = "Available";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public Event? Event { get; set; }
}

public class Booking
{
    public int BookingId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public int EventId { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime? HoldExpiresAt { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Customer? Customer { get; set; }
    public Event? Event { get; set; }
    public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
    public ParkingReservation? ParkingReservation { get; set; }
    public Payment? Payment { get; set; }
}

public class BookingSeat
{
    public int BookingSeatId { get; set; }
    public int BookingId { get; set; }
    public int SeatId { get; set; }
    public decimal PriceAtBooking { get; set; }
    public bool IsActive { get; set; } = true;
    public Booking? Booking { get; set; }
    public Seat? Seat { get; set; }
}

public class ParkingReservation
{
    public int ReservationId { get; set; }
    public int BookingId { get; set; }
    public int SlotId { get; set; }
    public decimal FeeAtReservation { get; set; }
    public bool IsActive { get; set; } = true;
    public Booking? Booking { get; set; }
    public ParkingSlot? Slot { get; set; }
}

public class Payment
{
    public int PaymentId { get; set; }
    public int BookingId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Completed";
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    public string ReceiptNumber { get; set; } = string.Empty;
    public Booking? Booking { get; set; }
}

public class Notification
{
    public int NotificationId { get; set; }
    public int CustomerId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Customer? Customer { get; set; }
}
