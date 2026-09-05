USE EventParkingReservationDb;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.EventCategories WHERE Name = N'Concert') INSERT dbo.EventCategories(Name) VALUES (N'Concert');
IF NOT EXISTS (SELECT 1 FROM dbo.EventCategories WHERE Name = N'Sports') INSERT dbo.EventCategories(Name) VALUES (N'Sports');
IF NOT EXISTS (SELECT 1 FROM dbo.EventCategories WHERE Name = N'Conference') INSERT dbo.EventCategories(Name) VALUES (N'Conference');
IF NOT EXISTS (SELECT 1 FROM dbo.EventCategories WHERE Name = N'Workshop') INSERT dbo.EventCategories(Name) VALUES (N'Workshop');

IF NOT EXISTS (SELECT 1 FROM dbo.Venues WHERE Name = N'Event Park Main Hall')
    INSERT dbo.Venues(Name, Address, Capacity) VALUES (N'Event Park Main Hall', N'Jaffna', 20);
GO

DECLARE @VenueId INT = (SELECT TOP 1 VenueId FROM dbo.Venues WHERE Name = N'Event Park Main Hall');
DECLARE @CategoryId INT = (SELECT TOP 1 CategoryId FROM dbo.EventCategories WHERE Name = N'Concert');

IF NOT EXISTS (SELECT 1 FROM dbo.Events WHERE Name = N'Demo Music Night')
BEGIN
    INSERT dbo.Events(Name, VenueId, CategoryId, EventDate, StartTime, EndTime, TicketPrice, ParkingFee, Capacity)
    VALUES (N'Demo Music Night', @VenueId, @CategoryId, DATEADD(DAY, 30, CAST(GETDATE() AS DATE)), '18:00', '21:00', 1500.00, 300.00, 20);
END;
GO

PRINT 'Sample venue/event added. Use the admin seat/parking APIs to generate layouts.';
