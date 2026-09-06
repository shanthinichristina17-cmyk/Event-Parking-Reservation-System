using System.Globalization;
using System.Text;
using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Repositories;
using Microsoft.Extensions.Options;

namespace EventParkingSystem.API.Services;

public interface IAdvancedAdminService
{
    Task<AnalyticsOverviewResponse> OverviewAsync();
    Task<List<MonthlyRevenueItem>> MonthlyRevenueAsync(int months);
    Task<List<TopEventAnalyticsItem>> TopEventsAsync(int limit);
    Task<List<CustomerGrowthItem>> CustomerGrowthAsync(int months);
    Task<List<RecentActivityItem>> RecentActivityAsync(int limit);
    Task<byte[]> ExportBookingsCsvAsync();
    Task<byte[]> ExportPaymentsCsvAsync();
    Task<byte[]> ExportEventsCsvAsync();
    Task<AdminSystemStatusResponse> SystemStatusAsync();
}

public sealed class AdvancedAdminService : IAdvancedAdminService
{
    private readonly IAdvancedAdminRepository _repo;
    private readonly BookingSettings _booking;
    private readonly IWebHostEnvironment _env;
    public AdvancedAdminService(IAdvancedAdminRepository repo,IOptions<BookingSettings> booking,IWebHostEnvironment env){_repo=repo;_booking=booking.Value;_env=env;}

    public async Task<AnalyticsOverviewResponse> OverviewAsync()
    {
        var bookings=await _repo.GetBookingsAsync(); var payments=await _repo.GetPaymentsAsync(); var refunds=await _repo.GetRefundsAsync();
        var customers=await _repo.GetCustomersAsync(); var events=await _repo.GetEventsAsync(); var today=DateOnly.FromDateTime(DateTime.Today);
        var confirmed=bookings.Count(x=>x.Status==BookingStatuses.Confirmed); var cancelled=bookings.Count(x=>x.Status==BookingStatuses.Cancelled);
        var gross=payments.Where(x=>x.Status==PaymentStatuses.Completed).Sum(x=>x.Amount); var refund=refunds.Sum(x=>x.Amount);
        return new AnalyticsOverviewResponse(bookings.Count,confirmed,cancelled,bookings.Count==0?0:Math.Round((decimal)confirmed/bookings.Count*100,2),bookings.Count==0?0:Math.Round((decimal)cancelled/bookings.Count*100,2),gross,refund,gross-refund,confirmed==0?0:Math.Round(gross/confirmed,2),customers.Count,customers.Count(x=>x.Status==CustomerStatuses.Active),events.Count,events.Count(x=>x.EventDate>=today));
    }

    public async Task<List<MonthlyRevenueItem>> MonthlyRevenueAsync(int months)
    {
        if(months<1||months>24) throw ApiException.BadRequest("months must be between 1 and 24.");
        var payments=await _repo.GetPaymentsAsync(); var refunds=await _repo.GetRefundsAsync(); var start=new DateTime(DateTime.UtcNow.Year,DateTime.UtcNow.Month,1).AddMonths(-(months-1));
        var list=new List<MonthlyRevenueItem>();
        for(int i=0;i<months;i++){var d=start.AddMonths(i);var r=payments.Where(x=>x.Status==PaymentStatuses.Completed&&x.PaidAt.Year==d.Year&&x.PaidAt.Month==d.Month).Sum(x=>x.Amount);var f=refunds.Where(x=>x.CreatedAt.Year==d.Year&&x.CreatedAt.Month==d.Month).Sum(x=>x.Amount);list.Add(new(d.Year,d.Month,d.ToString("MMM yyyy"),r,f,r-f));}
        return list;
    }

    public async Task<List<TopEventAnalyticsItem>> TopEventsAsync(int limit)
    {
        if(limit<1||limit>50) throw ApiException.BadRequest("limit must be between 1 and 50.");
        var b=await _repo.GetBookingsAsync();
        return b.Where(x=>x.Event!=null).GroupBy(x=>new{x.EventId,Name=x.Event!.Name,x.Event.EventDate}).Select(g=>new TopEventAnalyticsItem(g.Key.EventId,g.Key.Name,g.Key.EventDate,g.Count(x=>x.Status==BookingStatuses.Confirmed),g.Where(x=>x.Status==BookingStatuses.Confirmed).SelectMany(x=>x.BookingSeats).Count(x=>x.IsActive),g.Count(x=>x.Status==BookingStatuses.Confirmed&&x.ParkingReservation is {IsActive:true}),g.Where(x=>x.Payment?.Status==PaymentStatuses.Completed).Sum(x=>x.Payment!.Amount))).OrderByDescending(x=>x.Revenue).Take(limit).ToList();
    }

    public async Task<List<CustomerGrowthItem>> CustomerGrowthAsync(int months)
    {
        if(months<1||months>24) throw ApiException.BadRequest("months must be between 1 and 24.");
        var c=await _repo.GetCustomersAsync(); var start=new DateTime(DateTime.UtcNow.Year,DateTime.UtcNow.Month,1).AddMonths(-(months-1)); var cumulative=c.Count(x=>x.CreatedAt<start); var list=new List<CustomerGrowthItem>();
        for(int i=0;i<months;i++){var d=start.AddMonths(i);var n=c.Count(x=>x.CreatedAt.Year==d.Year&&x.CreatedAt.Month==d.Month);cumulative+=n;list.Add(new(d.Year,d.Month,d.ToString("MMM yyyy"),n,cumulative));}
        return list;
    }

    public async Task<List<RecentActivityItem>> RecentActivityAsync(int limit)
    {
        if(limit<1||limit>100) throw ApiException.BadRequest("limit must be between 1 and 100.");
        var b=await _repo.GetBookingsAsync(); var p=await _repo.GetPaymentsAsync(); var r=await _repo.GetRefundsAsync();
        return b.Select(x=>new RecentActivityItem("Booking",x.BookingId,x.BookingNumber,$"Booking {x.Status}",x.TotalAmount,x.CreatedAt))
            .Concat(p.Select(x=>new RecentActivityItem("Payment",x.PaymentId,x.ReceiptNumber,$"Payment {x.Status}",x.Amount,x.PaidAt)))
            .Concat(r.Select(x=>new RecentActivityItem("Refund",x.RefundId,$"REF-{x.RefundId}",$"Refund {x.Status}",x.Amount,x.CreatedAt)))
            .OrderByDescending(x=>x.OccurredAt).Take(limit).ToList();
    }

    public async Task<byte[]> ExportBookingsCsvAsync(){var rows=await _repo.GetBookingsAsync();var sb=new StringBuilder("BookingId,BookingNumber,Customer,Event,Status,Total,CreatedAt\n");foreach(var x in rows)sb.AppendLine($"{x.BookingId},{Csv(x.BookingNumber)},{Csv(x.Customer?.FullName)},{Csv(x.Event?.Name)},{Csv(x.Status)},{Money(x.TotalAmount)},{Csv(x.CreatedAt.ToString("O"))}");return Utf8Bom(sb.ToString());}
    public async Task<byte[]> ExportPaymentsCsvAsync(){var rows=await _repo.GetPaymentsAsync();var sb=new StringBuilder("PaymentId,BookingNumber,Customer,Event,Amount,Method,Status,Receipt,PaidAt\n");foreach(var x in rows)sb.AppendLine($"{x.PaymentId},{Csv(x.Booking?.BookingNumber)},{Csv(x.Booking?.Customer?.FullName)},{Csv(x.Booking?.Event?.Name)},{Money(x.Amount)},{Csv(x.PaymentMethod)},{Csv(x.Status)},{Csv(x.ReceiptNumber)},{Csv(x.PaidAt.ToString("O"))}");return Utf8Bom(sb.ToString());}
    public async Task<byte[]> ExportEventsCsvAsync(){var rows=await _repo.GetEventsAsync();var sb=new StringBuilder("EventId,Name,Venue,Category,Date,StartTime,EndTime,TicketPrice,ParkingFee,Capacity\n");foreach(var x in rows)sb.AppendLine($"{x.EventId},{Csv(x.Name)},{Csv(x.Venue?.Name)},{Csv(x.Category?.Name)},{Csv(x.EventDate.ToString("yyyy-MM-dd"))},{Csv(x.StartTime.ToString("HH:mm"))},{Csv(x.EndTime.ToString("HH:mm"))},{Money(x.TicketPrice)},{Money(x.ParkingFee)},{x.Capacity}");return Utf8Bom(sb.ToString());}

    public async Task<AdminSystemStatusResponse> SystemStatusAsync(){var b=await _repo.GetBookingsAsync();return new("ok",await _repo.CanConnectAsync(),_env.EnvironmentName,DateTime.UtcNow,Math.Max(1,_booking.HoldMinutes),Math.Max(1,_booking.CancellationCutoffHours),b.Count(x=>x.Status==BookingStatuses.Pending),b.Count(x=>x.Status==BookingStatuses.Expired),await _repo.CountUnreadNotificationsAsync());}

    static string Money(decimal v)=>v.ToString("0.00",CultureInfo.InvariantCulture);
    static string Csv(string? v)=>$"\"{(v??string.Empty).Replace("\"","\"\"")}\"";
    static byte[] Utf8Bom(string s)=>Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(s)).ToArray();
}

public interface IEventReminderService{Task<ReminderRunResponse> RunAsync(int withinHours=24);}
public sealed class EventReminderService:IEventReminderService
{
    readonly IAdvancedAdminRepository _repo; readonly INotificationService _notifications;
    public EventReminderService(IAdvancedAdminRepository repo,INotificationService notifications){_repo=repo;_notifications=notifications;}
    public async Task<ReminderRunResponse> RunAsync(int withinHours=24){if(withinHours<1||withinHours>168)throw ApiException.BadRequest("withinHours must be between 1 and 168.");var now=DateTime.Now;var end=now.AddHours(withinHours);var rows=await _repo.GetUpcomingConfirmedBookingsAsync(now,end);int eligible=0,created=0;foreach(var b in rows){if(b.Event is null)continue;var start=b.Event.EventDate.ToDateTime(b.Event.StartTime);if(start<=now||start>end)continue;eligible++;var venue=b.Event.Venue?.Name??"the venue";var msg=$"Reminder: {b.Event.Name} is on {b.Event.EventDate:yyyy-MM-dd} at {b.Event.StartTime:HH:mm} at {venue}. Booking {b.BookingNumber}.";var exists=await _repo.ReminderExistsAsync(b.CustomerId,msg);await _notifications.CreateIfMissingAsync(b.CustomerId,NotificationTypes.Reminder,msg);if(!exists)created++;}return new(eligible,created,withinHours,DateTime.UtcNow);}
}

public sealed class EventReminderHostedService:BackgroundService
{
    readonly IServiceScopeFactory _scope; readonly ILogger<EventReminderHostedService> _logger;
    public EventReminderHostedService(IServiceScopeFactory scope,ILogger<EventReminderHostedService> logger){_scope=scope;_logger=logger;}
    protected override async Task ExecuteAsync(CancellationToken stoppingToken){try{await Task.Delay(TimeSpan.FromSeconds(30),stoppingToken);}catch{return;}while(!stoppingToken.IsCancellationRequested){try{using var s=_scope.CreateScope();var svc=s.ServiceProvider.GetRequiredService<IEventReminderService>();var r=await svc.RunAsync(24);if(r.NotificationsCreated>0)_logger.LogInformation("Created {Count} event reminder(s).",r.NotificationsCreated);}catch(Exception ex){_logger.LogError(ex,"Event reminder job failed.");}try{await Task.Delay(TimeSpan.FromMinutes(30),stoppingToken);}catch{break;}}}
}
