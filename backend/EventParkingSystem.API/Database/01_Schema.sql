IF DB_ID(N'EventParkingReservationDb') IS NULL
BEGIN
    CREATE DATABASE EventParkingReservationDb;
END;
GO

USE EventParkingReservationDb;
GO

IF OBJECT_ID(N'dbo.Notifications', N'U') IS NOT NULL DROP TABLE dbo.Notifications;
IF OBJECT_ID(N'dbo.Payments', N'U') IS NOT NULL DROP TABLE dbo.Payments;
IF OBJECT_ID(N'dbo.ParkingReservations', N'U') IS NOT NULL DROP TABLE dbo.ParkingReservations;
IF OBJECT_ID(N'dbo.BookingSeats', N'U') IS NOT NULL DROP TABLE dbo.BookingSeats;
IF OBJECT_ID(N'dbo.Bookings', N'U') IS NOT NULL DROP TABLE dbo.Bookings;
IF OBJECT_ID(N'dbo.ParkingSlots', N'U') IS NOT NULL DROP TABLE dbo.ParkingSlots;
IF OBJECT_ID(N'dbo.Seats', N'U') IS NOT NULL DROP TABLE dbo.Seats;
IF OBJECT_ID(N'dbo.Events', N'U') IS NOT NULL DROP TABLE dbo.Events;
IF OBJECT_ID(N'dbo.EventCategories', N'U') IS NOT NULL DROP TABLE dbo.EventCategories;
IF OBJECT_ID(N'dbo.Venues', N'U') IS NOT NULL DROP TABLE dbo.Venues;
IF OBJECT_ID(N'dbo.Customers', N'U') IS NOT NULL DROP TABLE dbo.Customers;
GO

CREATE TABLE dbo.Customers
(
    CustomerId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Customers PRIMARY KEY,
    FullName NVARCHAR(150) NOT NULL,
    Email NVARCHAR(200) NOT NULL,
    Phone NVARCHAR(30) NULL,
    PasswordHash NVARCHAR(200) NOT NULL,
    Role NVARCHAR(20) NOT NULL CONSTRAINT DF_Customers_Role DEFAULT N'Customer',
    Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Customers_Status DEFAULT N'Active',
    EmailVerified BIT NOT NULL CONSTRAINT DF_Customers_EmailVerified DEFAULT 0,
    EmailVerificationToken NVARCHAR(128) NULL,
    EmailVerificationTokenExpiresAt DATETIME2 NULL,
    PasswordResetToken NVARCHAR(128) NULL,
    PasswordResetTokenExpiresAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Customers_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_Customers_UpdatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT CK_Customers_Role CHECK (Role IN (N'Customer', N'Admin')),
    CONSTRAINT CK_Customers_Status CHECK (Status IN (N'Active', N'Deactivated'))
);
CREATE UNIQUE INDEX UX_Customers_Email ON dbo.Customers(Email);
GO

CREATE TABLE dbo.Venues
(
    VenueId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Venues PRIMARY KEY,
    Name NVARCHAR(150) NOT NULL,
    Address NVARCHAR(300) NOT NULL,
    Capacity INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Venues_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_Venues_UpdatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT CK_Venues_Capacity CHECK (Capacity > 0)
);
GO

CREATE TABLE dbo.EventCategories
(
    CategoryId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EventCategories PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_EventCategories_CreatedAt DEFAULT SYSUTCDATETIME()
);
CREATE UNIQUE INDEX UX_EventCategories_Name ON dbo.EventCategories(Name);
GO

CREATE TABLE dbo.Events
(
    EventId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Events PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    VenueId INT NOT NULL,
    CategoryId INT NOT NULL,
    EventDate DATE NOT NULL,
    StartTime TIME(0) NOT NULL,
    EndTime TIME(0) NOT NULL,
    TicketPrice DECIMAL(10,2) NOT NULL,
    ParkingFee DECIMAL(10,2) NOT NULL,
    Capacity INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Events_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_Events_UpdatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Events_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues(VenueId),
    CONSTRAINT FK_Events_Categories FOREIGN KEY (CategoryId) REFERENCES dbo.EventCategories(CategoryId),
    CONSTRAINT CK_Events_Time CHECK (EndTime > StartTime),
    CONSTRAINT CK_Events_Price CHECK (TicketPrice >= 0 AND ParkingFee >= 0),
    CONSTRAINT CK_Events_Capacity CHECK (Capacity > 0)
);
CREATE INDEX IX_Events_EventDate ON dbo.Events(EventDate);
CREATE INDEX IX_Events_VenueId ON dbo.Events(VenueId);
CREATE INDEX IX_Events_CategoryId ON dbo.Events(CategoryId);
GO

CREATE TABLE dbo.Seats
(
    SeatId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Seats PRIMARY KEY,
    EventId INT NOT NULL,
    SeatRow NVARCHAR(10) NOT NULL,
    SeatNumber NVARCHAR(20) NOT NULL,
    SeatType NVARCHAR(50) NULL,
    Price DECIMAL(10,2) NOT NULL,
    Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Seats_Status DEFAULT N'Available',
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Seats_CreatedAt DEFAULT SYSUTCDATETIME(),
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_Seats_Events FOREIGN KEY (EventId) REFERENCES dbo.Events(EventId) ON DELETE CASCADE,
    CONSTRAINT CK_Seats_Status CHECK (Status IN (N'Available', N'Held', N'Booked')),
    CONSTRAINT CK_Seats_Price CHECK (Price >= 0)
);
CREATE UNIQUE INDEX UX_Seats_Event_Row_Number ON dbo.Seats(EventId, SeatRow, SeatNumber);
CREATE INDEX IX_Seats_Event_Status ON dbo.Seats(EventId, Status);
GO

CREATE TABLE dbo.ParkingSlots
(
    SlotId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ParkingSlots PRIMARY KEY,
    EventId INT NOT NULL,
    Zone NVARCHAR(30) NULL,
    SlotNumber NVARCHAR(30) NOT NULL,
    Fee DECIMAL(10,2) NOT NULL,
    Status NVARCHAR(20) NOT NULL CONSTRAINT DF_ParkingSlots_Status DEFAULT N'Available',
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_ParkingSlots_CreatedAt DEFAULT SYSUTCDATETIME(),
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_ParkingSlots_Events FOREIGN KEY (EventId) REFERENCES dbo.Events(EventId) ON DELETE CASCADE,
    CONSTRAINT CK_ParkingSlots_Status CHECK (Status IN (N'Available', N'Held', N'Reserved')),
    CONSTRAINT CK_ParkingSlots_Fee CHECK (Fee >= 0)
);
CREATE UNIQUE INDEX UX_ParkingSlots_Event_Slot ON dbo.ParkingSlots(EventId, SlotNumber);
CREATE INDEX IX_ParkingSlots_Event_Status ON dbo.ParkingSlots(EventId, Status);
GO

CREATE TABLE dbo.Bookings
(
    BookingId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Bookings PRIMARY KEY,
    BookingNumber NVARCHAR(40) NOT NULL,
    CustomerId INT NOT NULL,
    EventId INT NOT NULL,
    Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Bookings_Status DEFAULT N'Pending',
    HoldExpiresAt DATETIME2 NULL,
    TotalAmount DECIMAL(10,2) NOT NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Bookings_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_Bookings_UpdatedAt DEFAULT SYSUTCDATETIME(),
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_Bookings_Customers FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(CustomerId),
    CONSTRAINT FK_Bookings_Events FOREIGN KEY (EventId) REFERENCES dbo.Events(EventId),
    CONSTRAINT CK_Bookings_Status CHECK (Status IN (N'Pending', N'Confirmed', N'Cancelled', N'Expired')),
    CONSTRAINT CK_Bookings_Total CHECK (TotalAmount >= 0)
);
CREATE UNIQUE INDEX UX_Bookings_BookingNumber ON dbo.Bookings(BookingNumber);
CREATE INDEX IX_Bookings_CustomerId ON dbo.Bookings(CustomerId);
CREATE INDEX IX_Bookings_EventId ON dbo.Bookings(EventId);
CREATE INDEX IX_Bookings_Status_HoldExpiresAt ON dbo.Bookings(Status, HoldExpiresAt);
GO

CREATE TABLE dbo.BookingSeats
(
    BookingSeatId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BookingSeats PRIMARY KEY,
    BookingId INT NOT NULL,
    SeatId INT NOT NULL,
    PriceAtBooking DECIMAL(10,2) NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_BookingSeats_IsActive DEFAULT 1,
    CONSTRAINT FK_BookingSeats_Bookings FOREIGN KEY (BookingId) REFERENCES dbo.Bookings(BookingId) ON DELETE CASCADE,
    CONSTRAINT FK_BookingSeats_Seats FOREIGN KEY (SeatId) REFERENCES dbo.Seats(SeatId),
    CONSTRAINT CK_BookingSeats_Price CHECK (PriceAtBooking >= 0)
);
CREATE UNIQUE INDEX UX_BookingSeats_Booking_Seat ON dbo.BookingSeats(BookingId, SeatId);
CREATE UNIQUE INDEX UX_BookingSeats_ActiveSeat ON dbo.BookingSeats(SeatId) WHERE IsActive = 1;
GO

CREATE TABLE dbo.ParkingReservations
(
    ReservationId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ParkingReservations PRIMARY KEY,
    BookingId INT NOT NULL,
    SlotId INT NOT NULL,
    FeeAtReservation DECIMAL(10,2) NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_ParkingReservations_IsActive DEFAULT 1,
    CONSTRAINT FK_ParkingReservations_Bookings FOREIGN KEY (BookingId) REFERENCES dbo.Bookings(BookingId) ON DELETE CASCADE,
    CONSTRAINT FK_ParkingReservations_Slots FOREIGN KEY (SlotId) REFERENCES dbo.ParkingSlots(SlotId),
    CONSTRAINT CK_ParkingReservations_Fee CHECK (FeeAtReservation >= 0)
);
CREATE UNIQUE INDEX UX_ParkingReservations_Booking ON dbo.ParkingReservations(BookingId);
CREATE UNIQUE INDEX UX_ParkingReservations_ActiveSlot ON dbo.ParkingReservations(SlotId) WHERE IsActive = 1;
GO

CREATE TABLE dbo.Payments
(
    PaymentId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Payments PRIMARY KEY,
    BookingId INT NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    Status NVARCHAR(20) NOT NULL,
    PaidAt DATETIME2 NOT NULL,
    ReceiptNumber NVARCHAR(50) NOT NULL,
    CONSTRAINT FK_Payments_Bookings FOREIGN KEY (BookingId) REFERENCES dbo.Bookings(BookingId) ON DELETE CASCADE,
    CONSTRAINT CK_Payments_Amount CHECK (Amount >= 0),
    CONSTRAINT CK_Payments_Status CHECK (Status IN (N'Completed'))
);
CREATE UNIQUE INDEX UX_Payments_Booking ON dbo.Payments(BookingId);
CREATE UNIQUE INDEX UX_Payments_Receipt ON dbo.Payments(ReceiptNumber);
GO

CREATE TABLE dbo.Notifications
(
    NotificationId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Notifications PRIMARY KEY,
    CustomerId INT NOT NULL,
    Type NVARCHAR(30) NOT NULL,
    Message NVARCHAR(600) NOT NULL,
    IsRead BIT NOT NULL CONSTRAINT DF_Notifications_IsRead DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Notifications_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Notifications_Customers FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(CustomerId) ON DELETE CASCADE
);
CREATE INDEX IX_Notifications_Customer_CreatedAt ON dbo.Notifications(CustomerId, CreatedAt DESC);
GO

PRINT 'EventParkingReservationDb schema created successfully.';
