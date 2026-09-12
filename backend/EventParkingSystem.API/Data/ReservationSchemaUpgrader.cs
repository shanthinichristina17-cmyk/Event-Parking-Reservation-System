using Microsoft.EntityFrameworkCore;

namespace EventParkingSystem.API.Data;

public static class ReservationSchemaUpgrader
{
    public static async Task ApplyAsync(AppDbContext db)
    {
        var sql = """
IF COL_LENGTH('Bookings', 'TicketSubtotal') IS NULL
    ALTER TABLE [Bookings] ADD [TicketSubtotal] decimal(10,2) NOT NULL CONSTRAINT [DF_Bookings_TicketSubtotal] DEFAULT(0);

IF COL_LENGTH('Bookings', 'ParkingFee') IS NULL
    ALTER TABLE [Bookings] ADD [ParkingFee] decimal(10,2) NOT NULL CONSTRAINT [DF_Bookings_ParkingFee] DEFAULT(0);

IF COL_LENGTH('Bookings', 'PromoCode') IS NULL
    ALTER TABLE [Bookings] ADD [PromoCode] nvarchar(50) NULL;

IF COL_LENGTH('Bookings', 'DiscountAmount') IS NULL
    ALTER TABLE [Bookings] ADD [DiscountAmount] decimal(10,2) NOT NULL CONSTRAINT [DF_Bookings_DiscountAmount] DEFAULT(0);

IF COL_LENGTH('ParkingSlots', 'ParkingType') IS NULL
    ALTER TABLE [ParkingSlots] ADD [ParkingType] nvarchar(50) NULL;

IF COL_LENGTH('ParkingSlots', 'IsDisabled') IS NULL
    ALTER TABLE [ParkingSlots] ADD [IsDisabled] bit NOT NULL CONSTRAINT [DF_ParkingSlots_IsDisabled] DEFAULT(0);

IF COL_LENGTH('Payments', 'PaymentMethod') IS NULL
    ALTER TABLE [Payments] ADD [PaymentMethod] nvarchar(50) NOT NULL CONSTRAINT [DF_Payments_PaymentMethod] DEFAULT('Card');

IF COL_LENGTH('Payments', 'FailureReason') IS NULL
    ALTER TABLE [Payments] ADD [FailureReason] nvarchar(300) NULL;

-- Legacy SQL installs only allowed Reserved parking and completed payments.
-- Current services persist Booked/Disabled parking and retain failed attempts.
IF OBJECT_ID(N'[dbo].[CK_ParkingSlots_Status]', N'C') IS NOT NULL
    ALTER TABLE [dbo].[ParkingSlots] DROP CONSTRAINT [CK_ParkingSlots_Status];

UPDATE [dbo].[ParkingSlots]
SET [Status] = 'Booked'
WHERE [Status] = 'Reserved';

IF OBJECT_ID(N'[dbo].[CK_ParkingSlots_Status]', N'C') IS NULL
    ALTER TABLE [dbo].[ParkingSlots] WITH CHECK ADD CONSTRAINT [CK_ParkingSlots_Status]
        CHECK ([Status] IN (N'Available', N'Held', N'Booked', N'Disabled'));

IF OBJECT_ID(N'[dbo].[CK_Payments_Status]', N'C') IS NOT NULL
    ALTER TABLE [dbo].[Payments] DROP CONSTRAINT [CK_Payments_Status];

IF OBJECT_ID(N'[dbo].[CK_Payments_Status]', N'C') IS NULL
    ALTER TABLE [dbo].[Payments] WITH CHECK ADD CONSTRAINT [CK_Payments_Status]
        CHECK ([Status] IN (N'Pending', N'Completed', N'Failed'));

IF OBJECT_ID(N'[Refunds]', N'U') IS NULL
BEGIN
    CREATE TABLE [Refunds](
        [RefundId] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [BookingId] int NOT NULL,
        [PaymentId] int NULL,
        [Amount] decimal(10,2) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [Reason] nvarchar(300) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [FK_Refunds_Bookings_BookingId]
            FOREIGN KEY([BookingId]) REFERENCES [Bookings]([BookingId]) ON DELETE CASCADE,
        CONSTRAINT [FK_Refunds_Payments_PaymentId]
            FOREIGN KEY([PaymentId]) REFERENCES [Payments]([PaymentId])
    );

    CREATE UNIQUE INDEX [IX_Refunds_BookingId] ON [Refunds]([BookingId]);
    CREATE INDEX [IX_Refunds_PaymentId] ON [Refunds]([PaymentId]);
END;
""";

        await db.Database.ExecuteSqlRawAsync(sql);
    }
}
