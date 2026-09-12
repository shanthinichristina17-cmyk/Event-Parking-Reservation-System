using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Repositories;

namespace EventParkingSystem.API.Services;

public interface IAdminService
{
    Task<AdminDashboardResponse> GetDashboardAsync();
    Task<CustomerDashboardResponse> GetCustomerDashboardAsync(int customerId);
    Task<List<AdminBookingDto>> GetBookingsAsync(string? status, string? search);
    Task<List<AdminPaymentDto>> GetPaymentsAsync();
    Task<ReportSummaryResponse> GetReportSummaryAsync(DateTime? from, DateTime? to);
    Task<List<RevenueByEventDto>> GetRevenueByEventAsync(DateTime? from, DateTime? to);
    Task<List<BookingStatusDto>> GetBookingStatusBreakdownAsync(DateTime? from, DateTime? to);
    Task<int> GetUnreadNotificationCountAsync(int customerId);
    Task BroadcastNotificationAsync(BroadcastNotificationRequest request);
}

public sealed class AdminService : IAdminService
{
    private readonly IAdminRepository _repo;

    public AdminService(IAdminRepository repo) => _repo = repo;

    public Task<AdminDashboardResponse> GetDashboardAsync() =>
        _repo.GetDashboardAsync();

    public Task<CustomerDashboardResponse> GetCustomerDashboardAsync(int customerId) =>
        _repo.GetCustomerDashboardAsync(customerId);

    public Task<List<AdminBookingDto>> GetBookingsAsync(string? status, string? search) =>
        _repo.GetBookingsAsync(status, search);

    public Task<List<AdminPaymentDto>> GetPaymentsAsync() =>
        _repo.GetPaymentsAsync();

    public Task<ReportSummaryResponse> GetReportSummaryAsync(DateTime? from, DateTime? to)
    {
        ValidateDates(from, to);
        return _repo.GetReportSummaryAsync(from, to);
    }

    public Task<List<RevenueByEventDto>> GetRevenueByEventAsync(DateTime? from, DateTime? to)
    {
        ValidateDates(from, to);
        return _repo.GetRevenueByEventAsync(from, to);
    }

    public Task<List<BookingStatusDto>> GetBookingStatusBreakdownAsync(DateTime? from, DateTime? to)
    {
        ValidateDates(from, to);
        return _repo.GetBookingStatusBreakdownAsync(from, to);
    }

    public Task<int> GetUnreadNotificationCountAsync(int customerId) =>
        _repo.GetUnreadNotificationCountAsync(customerId);

    public async Task BroadcastNotificationAsync(BroadcastNotificationRequest request)
    {
        var type = string.IsNullOrWhiteSpace(request.Type)
            ? NotificationTypes.Update
            : request.Type.Trim();

        if (type.Length > 30)
            throw ApiException.BadRequest("Notification type cannot exceed 30 characters.");

        if (string.IsNullOrWhiteSpace(request.Message))
            throw ApiException.BadRequest("Notification message is required.");

        if (request.Message.Trim().Length > 600)
            throw ApiException.BadRequest("Notification message cannot exceed 600 characters.");

        await _repo.BroadcastNotificationAsync(type, request.Message.Trim());
    }

    private static void ValidateDates(DateTime? from, DateTime? to)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
            throw ApiException.BadRequest("'from' date cannot be after 'to' date.");
    }
}
