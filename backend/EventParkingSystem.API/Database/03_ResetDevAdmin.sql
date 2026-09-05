USE EventParkingReservationDb;
GO

-- Optional recovery script for an old development database.
-- Normally v4 DbSeeder repairs the demo admin automatically at API startup.
-- This script only removes the old demo admin so the seeder can recreate it.
DELETE FROM dbo.Customers
WHERE Email = N'admin@eventpark.local'
  AND NOT EXISTS (SELECT 1 FROM dbo.Bookings b WHERE b.CustomerId = dbo.Customers.CustomerId);
GO
