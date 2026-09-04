using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Repositories;

namespace EventParkingSystem.API.Services;

public interface ICustomerService
{
    Task<CustomerProfileWithBookingsSummaryResponse> GetAsync(int id);
    Task<PagedResponse<CustomerListItemResponse>> SearchAsync(string? search, int page, int pageSize);
    Task<CustomerProfileResponse> UpdateAsync(int id, UpdateCustomerProfileRequest request);
    Task DeactivateAsync(int id);
    Task ReactivateAsync(int id);
}

public sealed class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customers;
    public CustomerService(ICustomerRepository customers) => _customers = customers;

    public async Task<CustomerProfileWithBookingsSummaryResponse> GetAsync(int id)
    {
        var customer = await _customers.GetByIdAsync(id) ?? throw ApiException.NotFound("Customer not found.");
        var summary = await _customers.GetBookingSummaryAsync(id);
        return new CustomerProfileWithBookingsSummaryResponse(
            AuthService.MapProfile(customer), summary.Total, summary.Upcoming, summary.Cancelled);
    }

    public async Task<PagedResponse<CustomerListItemResponse>> SearchAsync(string? search, int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var result = await _customers.SearchAsync(search, page, pageSize);
        var items = result.Items.Select(x => new CustomerListItemResponse(
            x.CustomerId, x.FullName, x.Email, x.Phone, x.Status, x.EmailVerified, x.CreatedAt)).ToList();
        return new PagedResponse<CustomerListItemResponse>(items, page, pageSize, result.Total);
    }

    public async Task<CustomerProfileResponse> UpdateAsync(int id, UpdateCustomerProfileRequest request)
    {
        var customer = await _customers.GetByIdAsync(id) ?? throw ApiException.NotFound("Customer not found.");
        customer.FullName = request.FullName.Trim();
        customer.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        customer.UpdatedAt = DateTime.UtcNow;
        await _customers.SaveChangesAsync();
        return AuthService.MapProfile(customer);
    }

    public async Task DeactivateAsync(int id)
    {
        var customer = await _customers.GetByIdAsync(id) ?? throw ApiException.NotFound("Customer not found.");
        if (customer.Role == Roles.Admin)
            throw ApiException.BadRequest("Admin accounts cannot be deactivated from Customer Management.");
        if (await _customers.HasActiveFutureBookingsAsync(id))
            throw ApiException.Conflict("Customer has an active future booking and cannot be deactivated.");

        customer.Status = CustomerStatuses.Deactivated;
        customer.UpdatedAt = DateTime.UtcNow;
        await _customers.SaveChangesAsync();
    }

    public async Task ReactivateAsync(int id)
    {
        var customer = await _customers.GetByIdAsync(id) ?? throw ApiException.NotFound("Customer not found.");
        customer.Status = CustomerStatuses.Active;
        customer.UpdatedAt = DateTime.UtcNow;
        await _customers.SaveChangesAsync();
    }
}
