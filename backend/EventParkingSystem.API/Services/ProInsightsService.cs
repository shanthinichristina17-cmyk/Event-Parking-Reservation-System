using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Models;
using EventParkingSystem.API.Repositories;

namespace EventParkingSystem.API.Services;

public interface IProInsightsService
{
    Task<EventDiscoveryResponse> DiscoverEventsAsync(
        string? search,
        int? categoryId,
        int? venueId,
        DateOnly? from,
        DateOnly? to,
        decimal? maxTicketPrice,
        bool? parkingRequired,
        string sort,
        int page,
        int pageSize);

    Task<EventAvailabilityResponse> GetAvailabilityAsync(int eventId);
    Task<BookingPriceEstimateResponse> EstimateAsync(
        int eventId,
        int seatCount,
        bool includeParking);
    Task<BookingReadinessResponse> GetBookingReadinessAsync(int customerId);
    Task<CustomerSavingsResponse> GetSavingsAsync(int customerId);
    Task<List<SimilarEventResponse>> GetSimilarEventsAsync(int eventId, int limit);

    Task<List<AdminOperationsAlertResponse>> GetOperationsAlertsAsync();
    Task<List<EventCapacityUtilizationResponse>> GetCapacityUtilizationAsync();
    Task<PaymentHealthResponse> GetPaymentHealthAsync(int days);
    Task<List<EventRevenueOpportunityItem>> GetRevenueOpportunityAsync();
    Task<CustomerSegmentsResponse> GetCustomerSegmentsAsync();
    Task<List<EventReadinessResponse>> GetEventReadinessAsync(int days);
}

public sealed class ProInsightsService : IProInsightsService
{
    private readonly IProInsightsRepository _repo;

    public ProInsightsService(IProInsightsRepository repo) => _repo = repo;

    public async Task<EventDiscoveryResponse> DiscoverEventsAsync(
        string? search,
        int? categoryId,
        int? venueId,
        DateOnly? from,
        DateOnly? to,
        decimal? maxTicketPrice,
        bool? parkingRequired,
        string sort,
        int page,
        int pageSize)
    {
        if (page < 1)
            throw ApiException.BadRequest("page must be at least 1.");
        if (pageSize is < 1 or > 50)
            throw ApiException.BadRequest("pageSize must be between 1 and 50.");
        if (from.HasValue && to.HasValue && from.Value > to.Value)
            throw ApiException.BadRequest("from cannot be after to.");
        if (maxTicketPrice.HasValue && maxTicketPrice.Value < 0)
            throw ApiException.BadRequest("maxTicketPrice cannot be negative.");

        var today = DateOnly.FromDateTime(DateTime.Today);
        var events = await _repo.GetEventsDetailedAsync();

        IEnumerable<Event> query = events.Where(x => x.EventDate >= today);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                x.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (x.Venue?.Name?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.Category?.Name?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        if (categoryId.HasValue)
            query = query.Where(x => x.CategoryId == categoryId.Value);
        if (venueId.HasValue)
            query = query.Where(x => x.VenueId == venueId.Value);
        if (from.HasValue)
            query = query.Where(x => x.EventDate >= from.Value);
        if (to.HasValue)
            query = query.Where(x => x.EventDate <= to.Value);
        if (maxTicketPrice.HasValue)
            query = query.Where(x => x.TicketPrice <= maxTicketPrice.Value);
        if (parkingRequired == true)
            query = query.Where(x => x.ParkingSlots.Any(p =>
                !p.IsDisabled && p.Status == ParkingStatuses.Available));

        query = sort.Trim().ToLowerInvariant() switch
        {
            "price" => query.OrderBy(x => x.TicketPrice).ThenBy(x => x.EventDate),
            "price-desc" => query.OrderByDescending(x => x.TicketPrice).ThenBy(x => x.EventDate),
            "availability" => query.OrderByDescending(x =>
                x.Seats.Count(s => s.Status == SeatStatuses.Available))
                .ThenBy(x => x.EventDate),
            _ => query.OrderBy(x => x.EventDate).ThenBy(x => x.StartTime)
        };

        var all = query.ToList();
        var totalItems = all.Count;
        var totalPages = totalItems == 0
            ? 0
            : (int)Math.Ceiling(totalItems / (double)pageSize);

        var items = all
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ToDiscoveryItem)
            .ToList();

        return new EventDiscoveryResponse(
            page,
            pageSize,
            totalItems,
            totalPages,
            items);
    }

    public async Task<EventAvailabilityResponse> GetAvailabilityAsync(int eventId)
    {
        var ev = await GetEventAsync(eventId);

        var totalSeats = ev.Seats.Count;
        var availableSeats = ev.Seats.Count(x => x.Status == SeatStatuses.Available);
        var heldSeats = ev.Seats.Count(x => x.Status == SeatStatuses.Held);
        var bookedSeats = ev.Seats.Count(x => x.Status == SeatStatuses.Booked);

        var enabledParking = ev.ParkingSlots.Where(x => !x.IsDisabled).ToList();
        var availableParking = enabledParking.Count(x => x.Status == ParkingStatuses.Available);
        var heldParking = enabledParking.Count(x => x.Status == ParkingStatuses.Held);
        var bookedParking = enabledParking.Count(x => x.Status == ParkingStatuses.Booked);

        var availableSeatPrices = ev.Seats
            .Where(x => x.Status == SeatStatuses.Available)
            .Select(x => x.Price)
            .ToList();

        var availableParkingFees = enabledParking
            .Where(x => x.Status == ParkingStatuses.Available)
            .Select(x => x.Fee)
            .ToList();

        return new EventAvailabilityResponse(
            ev.EventId,
            ev.Name,
            ev.EventDate,
            ev.StartTime,
            ev.Capacity,
            totalSeats,
            availableSeats,
            heldSeats,
            bookedSeats,
            Percentage(bookedSeats, totalSeats),
            availableSeatPrices.Count == 0 ? null : availableSeatPrices.Min(),
            availableSeatPrices.Count == 0 ? null : availableSeatPrices.Max(),
            enabledParking.Count,
            availableParking,
            heldParking,
            bookedParking,
            Percentage(bookedParking, enabledParking.Count),
            availableParkingFees.Count == 0 ? null : availableParkingFees.Min(),
            availableSeats > 0,
            availableParking > 0);
    }

    public async Task<BookingPriceEstimateResponse> EstimateAsync(
        int eventId,
        int seatCount,
        bool includeParking)
    {
        if (seatCount is < 1 or > 20)
            throw ApiException.BadRequest("seatCount must be between 1 and 20.");

        var ev = await GetEventAsync(eventId);

        var availableSeatPrices = ev.Seats
            .Where(x => x.Status == SeatStatuses.Available)
            .OrderBy(x => x.Price)
            .Select(x => x.Price)
            .ToList();

        var canBookSeats = availableSeatPrices.Count >= seatCount;

        var availableParkingFees = ev.ParkingSlots
            .Where(x => !x.IsDisabled && x.Status == ParkingStatuses.Available)
            .OrderBy(x => x.Fee)
            .Select(x => x.Fee)
            .ToList();

        var canBookParking = !includeParking || availableParkingFees.Count > 0;
        var canBook = canBookSeats && canBookParking;

        var seatSubtotal = canBookSeats
            ? availableSeatPrices.Take(seatCount).Sum()
            : 0m;

        var parkingFee = includeParking && availableParkingFees.Count > 0
            ? availableParkingFees[0]
            : 0m;

        var note = canBook
            ? "Estimate uses the cheapest currently available seats and parking slot. Final total depends on the exact selected resources and promo code."
            : !canBookSeats
                ? $"Only {availableSeatPrices.Count} seat(s) are currently available."
                : "No parking slot is currently available.";

        return new BookingPriceEstimateResponse(
            ev.EventId,
            ev.Name,
            seatCount,
            includeParking,
            canBook,
            seatSubtotal,
            parkingFee,
            seatSubtotal + parkingFee,
            note);
    }

    public async Task<BookingReadinessResponse> GetBookingReadinessAsync(int customerId)
    {
        var bookings = await _repo.GetCustomerBookingsAsync(customerId);
        var unread = await _repo.GetUnreadNotificationsAsync(customerId);
        var nowUtc = DateTime.UtcNow;
        var nowLocal = DateTime.Now;

        var pending = bookings.Count(x => x.Status == BookingStatuses.Pending);
        var activeHolds = bookings
            .Where(x =>
                x.Status == BookingStatuses.Pending &&
                x.HoldExpiresAt.HasValue &&
                x.HoldExpiresAt.Value > nowUtc)
            .ToList();

        var nextHoldExpiry = activeHolds
            .Where(x => x.HoldExpiresAt.HasValue)
            .Select(x => x.HoldExpiresAt!.Value)
            .OrderBy(x => x)
            .Cast<DateTime?>()
            .FirstOrDefault();

        var upcomingConfirmed = bookings.Count(x =>
            x.Status == BookingStatuses.Confirmed &&
            x.Event != null &&
            x.Event.EventDate.ToDateTime(x.Event.StartTime) >= nowLocal);

        var status = activeHolds.Count > 0
            ? "ActionRequired"
            : pending > 0
                ? "Pending"
                : "Ready";

        var message = activeHolds.Count > 0
            ? "You have an active booking hold. Complete payment before it expires."
            : pending > 0
                ? "You have pending booking activity to review."
                : upcomingConfirmed > 0
                    ? "Your upcoming confirmed bookings are ready."
                    : "You can discover and book an upcoming event.";

        return new BookingReadinessResponse(
            pending,
            activeHolds.Count,
            nextHoldExpiry,
            upcomingConfirmed,
            unread,
            status,
            message);
    }

    public async Task<CustomerSavingsResponse> GetSavingsAsync(int customerId)
    {
        var bookings = await _repo.GetCustomerBookingsAsync(customerId);

        var promoBookings = bookings.Count(x => x.DiscountAmount > 0m);
        var totalSavings = bookings.Sum(x => x.DiscountAmount);
        var average = promoBookings == 0
            ? 0m
            : Math.Round(totalSavings / promoBookings, 2);
        var refunded = bookings
            .Where(x => x.Refund != null)
            .Sum(x => x.Refund!.Amount);

        return new CustomerSavingsResponse(
            promoBookings,
            totalSavings,
            average,
            refunded,
            totalSavings + refunded);
    }

    public async Task<List<SimilarEventResponse>> GetSimilarEventsAsync(
        int eventId,
        int limit)
    {
        if (limit is < 1 or > 20)
            throw ApiException.BadRequest("limit must be between 1 and 20.");

        var target = await GetEventAsync(eventId);
        var events = await _repo.GetEventsDetailedAsync();
        var today = DateOnly.FromDateTime(DateTime.Today);

        return events
            .Where(x =>
                x.EventId != eventId &&
                x.CategoryId == target.CategoryId &&
                x.EventDate >= today)
            .OrderBy(x => x.EventDate)
            .ThenBy(x => x.StartTime)
            .Take(limit)
            .Select(x => new SimilarEventResponse(
                x.EventId,
                x.Name,
                x.Venue?.Name ?? string.Empty,
                x.Category?.Name ?? string.Empty,
                x.EventDate,
                x.StartTime,
                x.TicketPrice,
                x.Seats.Count(s => s.Status == SeatStatuses.Available)))
            .ToList();
    }

    public async Task<List<AdminOperationsAlertResponse>> GetOperationsAlertsAsync()
    {
        var events = await _repo.GetEventsDetailedAsync();
        var bookings = await _repo.GetAllBookingsDetailedAsync();
        var payments = await _repo.GetPaymentsSinceAsync(DateTime.UtcNow.AddHours(-24));
        var nowUtc = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(DateTime.Today);
        var next14Days = today.AddDays(14);

        var alerts = new List<AdminOperationsAlertResponse>();

        foreach (var ev in events.Where(x => x.EventDate >= today && x.EventDate <= next14Days))
        {
            if (ev.Seats.Count == 0)
            {
                alerts.Add(new AdminOperationsAlertResponse(
                    "High",
                    "SeatMapMissing",
                    ev.EventId,
                    null,
                    $"Seat map missing for {ev.Name}",
                    "Generate seats before customers start booking.",
                    nowUtc));
                continue;
            }

            var available = ev.Seats.Count(x => x.Status == SeatStatuses.Available);
            var ratio = available / (double)ev.Seats.Count;

            if (available == 0)
            {
                alerts.Add(new AdminOperationsAlertResponse(
                    "Info",
                    "SoldOut",
                    ev.EventId,
                    null,
                    $"{ev.Name} has no available seats",
                    "The event is currently sold out.",
                    nowUtc));
            }
            else if (ratio <= 0.15)
            {
                alerts.Add(new AdminOperationsAlertResponse(
                    "Medium",
                    "LowSeatAvailability",
                    ev.EventId,
                    null,
                    $"Low seat availability for {ev.Name}",
                    $"Only {available} of {ev.Seats.Count} generated seats are available.",
                    nowUtc));
            }

            if (ev.Seats.Count != ev.Capacity)
            {
                alerts.Add(new AdminOperationsAlertResponse(
                    "High",
                    "CapacityMismatch",
                    ev.EventId,
                    null,
                    $"Seat capacity mismatch for {ev.Name}",
                    $"Event capacity is {ev.Capacity}, but {ev.Seats.Count} seats are generated.",
                    nowUtc));
            }
        }

        foreach (var booking in bookings.Where(x =>
            x.Status == BookingStatuses.Pending &&
            x.HoldExpiresAt.HasValue &&
            x.HoldExpiresAt.Value > nowUtc &&
            x.HoldExpiresAt.Value <= nowUtc.AddMinutes(10)))
        {
            alerts.Add(new AdminOperationsAlertResponse(
                "Low",
                "HoldExpiringSoon",
                booking.EventId,
                booking.BookingId,
                $"Booking hold {booking.BookingNumber} expires soon",
                $"Hold expires at {booking.HoldExpiresAt:O}.",
                nowUtc));
        }

        var failedPayments = payments
            .Where(x => x.Status == PaymentStatuses.Failed)
            .ToList();

        if (failedPayments.Count > 0)
        {
            alerts.Add(new AdminOperationsAlertResponse(
                failedPayments.Count >= 5 ? "High" : "Medium",
                "PaymentFailures",
                null,
                null,
                "Payment failures detected in the last 24 hours",
                $"{failedPayments.Count} failed payment attempt(s), total amount {failedPayments.Sum(x => x.Amount):0.00}.",
                nowUtc));
        }

        return alerts
            .OrderBy(x => SeverityOrder(x.Severity))
            .ThenBy(x => x.Type)
            .ToList();
    }

    public async Task<List<EventCapacityUtilizationResponse>> GetCapacityUtilizationAsync()
    {
        var events = await _repo.GetEventsDetailedAsync();

        return events
            .OrderBy(x => x.EventDate)
            .ThenBy(x => x.StartTime)
            .Select(x =>
            {
                var seats = x.Seats.ToList();
                var enabledParking = x.ParkingSlots.Where(p => !p.IsDisabled).ToList();
                var bookedSeats = seats.Count(s => s.Status == SeatStatuses.Booked);
                var bookedParking = enabledParking.Count(p => p.Status == ParkingStatuses.Booked);

                return new EventCapacityUtilizationResponse(
                    x.EventId,
                    x.Name,
                    x.EventDate,
                    x.Capacity,
                    seats.Count,
                    seats.Count(s => s.Status == SeatStatuses.Available),
                    seats.Count(s => s.Status == SeatStatuses.Held),
                    bookedSeats,
                    Percentage(bookedSeats, seats.Count),
                    enabledParking.Count,
                    enabledParking.Count(p => p.Status == ParkingStatuses.Available),
                    enabledParking.Count(p => p.Status == ParkingStatuses.Held),
                    bookedParking,
                    Percentage(bookedParking, enabledParking.Count));
            })
            .ToList();
    }

    public async Task<PaymentHealthResponse> GetPaymentHealthAsync(int days)
    {
        if (days is < 1 or > 365)
            throw ApiException.BadRequest("days must be between 1 and 365.");

        var payments = await _repo.GetPaymentsSinceAsync(DateTime.UtcNow.AddDays(-days));
        var completed = payments.Where(x => x.Status == PaymentStatuses.Completed).ToList();
        var failed = payments.Where(x => x.Status == PaymentStatuses.Failed).ToList();
        var pending = payments.Where(x => x.Status == PaymentStatuses.Pending).ToList();
        var resolved = completed.Count + failed.Count;

        var reasons = failed
            .GroupBy(x => string.IsNullOrWhiteSpace(x.FailureReason)
                ? "Unspecified"
                : x.FailureReason!.Trim())
            .Select(g => new PaymentFailureReasonResponse(
                g.Key,
                g.Count(),
                g.Sum(x => x.Amount)))
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Reason)
            .ToList();

        return new PaymentHealthResponse(
            days,
            payments.Count,
            completed.Count,
            failed.Count,
            pending.Count,
            resolved == 0
                ? 0m
                : Math.Round((decimal)completed.Count / resolved * 100m, 2),
            completed.Sum(x => x.Amount),
            failed.Sum(x => x.Amount),
            reasons);
    }

    public async Task<List<EventRevenueOpportunityItem>> GetRevenueOpportunityAsync()
    {
        var events = await _repo.GetEventsDetailedAsync();
        var today = DateOnly.FromDateTime(DateTime.Today);

        return events
            .Where(x => x.EventDate >= today)
            .Select(x =>
            {
                var realized = x.Bookings
                    .Where(b => b.Payment?.Status == PaymentStatuses.Completed)
                    .Sum(b => b.Payment!.Amount);

                var seatPotential = x.Seats
                    .Where(s => s.Status == SeatStatuses.Available)
                    .Sum(s => s.Price);

                var parkingPotential = x.ParkingSlots
                    .Where(p =>
                        !p.IsDisabled &&
                        p.Status == ParkingStatuses.Available)
                    .Sum(p => p.Fee);

                var remaining = seatPotential + parkingPotential;

                return new EventRevenueOpportunityItem(
                    x.EventId,
                    x.Name,
                    x.EventDate,
                    realized,
                    seatPotential,
                    parkingPotential,
                    remaining,
                    realized + remaining);
            })
            .OrderByDescending(x => x.RemainingPotential)
            .ThenBy(x => x.EventDate)
            .ToList();
    }

    public async Task<CustomerSegmentsResponse> GetCustomerSegmentsAsync()
    {
        var customers = await _repo.GetCustomersAsync();
        var nowUtc = DateTime.UtcNow;
        var nowLocal = DateTime.Now;

        var spendByCustomer = customers
            .Select(c => new
            {
                Customer = c,
                Spend = c.Bookings
                    .Where(b => b.Payment?.Status == PaymentStatuses.Completed)
                    .Sum(b => b.Payment!.Amount),
                Confirmed = c.Bookings.Count(b => b.Status == BookingStatuses.Confirmed),
                HasUpcoming = c.Bookings.Any(b =>
                    b.Status == BookingStatuses.Confirmed &&
                    b.Event != null &&
                    b.Event.EventDate.ToDateTime(b.Event.StartTime) >= nowLocal),
                LastBookingAt = c.Bookings
                    .Select(b => (DateTime?)b.CreatedAt)
                    .OrderByDescending(x => x)
                    .FirstOrDefault()
            })
            .ToList();

        var positiveSpends = spendByCustomer
            .Where(x => x.Spend > 0m)
            .Select(x => x.Spend)
            .OrderBy(x => x)
            .ToList();

        decimal highValueThreshold = 0m;
        if (positiveSpends.Count > 0)
        {
            var index = (int)Math.Floor((positiveSpends.Count - 1) * 0.75);
            highValueThreshold = positiveSpends[index];
        }

        var highValueCount = highValueThreshold <= 0m
            ? 0
            : spendByCustomer.Count(x => x.Spend >= highValueThreshold && x.Spend > 0m);

        var dormantCutoff = nowUtc.AddDays(-60);

        return new CustomerSegmentsResponse(
            customers.Count,
            customers.Count(x => x.CreatedAt >= nowUtc.AddDays(-30)),
            spendByCustomer.Count(x => x.Confirmed >= 2),
            highValueCount,
            spendByCustomer.Count(x => x.HasUpcoming),
            spendByCustomer.Count(x =>
                !x.LastBookingAt.HasValue || x.LastBookingAt.Value < dormantCutoff),
            highValueThreshold <= 0m
                ? "No completed-payment history yet"
                : $"Customers at or above the 75th percentile of completed spend (current threshold {highValueThreshold:0.00})",
            "No booking created in the last 60 days, or no booking history");
    }

    public async Task<List<EventReadinessResponse>> GetEventReadinessAsync(int days)
    {
        if (days is < 1 or > 365)
            throw ApiException.BadRequest("days must be between 1 and 365.");

        var events = await _repo.GetEventsDetailedAsync();
        var today = DateOnly.FromDateTime(DateTime.Today);
        var until = today.AddDays(days);

        return events
            .Where(x => x.EventDate >= today && x.EventDate <= until)
            .OrderBy(x => x.EventDate)
            .ThenBy(x => x.StartTime)
            .Select(x =>
            {
                var issues = new List<string>();
                var seatMapReady = x.Seats.Count > 0;
                var capacityMatches = x.Seats.Count == x.Capacity;

                if (!seatMapReady)
                    issues.Add("Seat map has not been generated.");
                if (seatMapReady && !capacityMatches)
                    issues.Add($"Generated seats ({x.Seats.Count}) do not match event capacity ({x.Capacity}).");
                if (x.ParkingFee > 0m && x.ParkingSlots.Count == 0)
                    issues.Add("Parking fee is configured but no parking slots are generated.");
                if (x.EndTime <= x.StartTime)
                    issues.Add("Event end time is not after the start time.");

                var status = issues.Count == 0
                    ? "Ready"
                    : issues.Any(i => i.Contains("Seat map", StringComparison.OrdinalIgnoreCase) ||
                                      i.Contains("capacity", StringComparison.OrdinalIgnoreCase))
                        ? "ActionRequired"
                        : "Review";

                return new EventReadinessResponse(
                    x.EventId,
                    x.Name,
                    x.EventDate,
                    x.StartTime,
                    x.Capacity,
                    x.Seats.Count,
                    x.ParkingSlots.Count,
                    seatMapReady,
                    capacityMatches,
                    status,
                    issues);
            })
            .ToList();
    }

    private async Task<Event> GetEventAsync(int eventId) =>
        await _repo.GetEventDetailedAsync(eventId)
        ?? throw ApiException.NotFound("Event not found.");

    private static EventDiscoveryItemResponse ToDiscoveryItem(Event x)
    {
        var availableSeats = x.Seats.Count(s => s.Status == SeatStatuses.Available);
        var availableSeatPrices = x.Seats
            .Where(s => s.Status == SeatStatuses.Available)
            .Select(s => s.Price)
            .ToList();
        var availableParking = x.ParkingSlots.Count(p =>
            !p.IsDisabled && p.Status == ParkingStatuses.Available);

        return new EventDiscoveryItemResponse(
            x.EventId,
            x.Name,
            x.VenueId,
            x.Venue?.Name ?? string.Empty,
            x.CategoryId,
            x.Category?.Name ?? string.Empty,
            x.EventDate,
            x.StartTime,
            x.EndTime,
            x.TicketPrice,
            x.ParkingFee,
            x.Capacity,
            availableSeats,
            x.Seats.Count(s => s.Status == SeatStatuses.Held),
            x.Seats.Count(s => s.Status == SeatStatuses.Booked),
            availableParking,
            availableSeatPrices.Count == 0 ? null : availableSeatPrices.Min(),
            availableParking > 0);
    }

    private static decimal Percentage(int numerator, int denominator) =>
        denominator <= 0
            ? 0m
            : Math.Round((decimal)numerator / denominator * 100m, 2);

    private static int SeverityOrder(string severity) => severity switch
    {
        "High" => 0,
        "Medium" => 1,
        "Low" => 2,
        _ => 3
    };
}
