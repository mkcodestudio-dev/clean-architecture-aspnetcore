using Booking.Application;
using Booking.Infrastructure;
using Xunit;

namespace Booking.Application.Tests;

public class ReservationServiceTests
{
    private static readonly DateOnly Today = new(2026, 12, 1);

    private readonly ReservationService _service = new(
        new InMemoryReservationRepository(),
        new FixedTimeProvider(new DateTimeOffset(2026, 12, 1, 9, 0, 0, TimeSpan.Zero)));

    private static CreateReservationRequest Request(string room, int fromDay, int toDay, string guest = "Ada Lovelace") =>
        new(room, guest, Today.AddDays(fromDay), Today.AddDays(toDay));

    [Fact]
    public async Task Create_with_valid_dates_confirms_the_reservation()
    {
        var result = await _service.CreateAsync(Request("101", 1, 4));

        Assert.True(result.IsSuccess);
        Assert.Equal("Confirmed", result.Value!.Status);
        Assert.Equal(3, result.Value.Nights);
    }

    [Fact]
    public async Task Create_rejects_overlapping_dates_for_the_same_room()
    {
        await _service.CreateAsync(Request("101", 1, 4));

        var result = await _service.CreateAsync(Request("101", 3, 6, "Grace Hopper"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Conflict, result.Error!.Kind);
    }

    [Fact]
    public async Task Create_allows_a_stay_that_starts_on_the_previous_check_out_day()
    {
        await _service.CreateAsync(Request("101", 1, 4));

        var result = await _service.CreateAsync(Request("101", 4, 6, "Grace Hopper"));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Create_allows_the_same_dates_in_a_different_room()
    {
        await _service.CreateAsync(Request("101", 1, 4));

        var result = await _service.CreateAsync(Request("102", 1, 4, "Grace Hopper"));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Create_rejects_check_out_before_check_in()
    {
        var result = await _service.CreateAsync(Request("101", 5, 2));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Validation, result.Error!.Kind);
    }

    [Fact]
    public async Task Create_rejects_check_in_in_the_past()
    {
        var result = await _service.CreateAsync(Request("101", -2, 1));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Validation, result.Error!.Kind);
    }

    [Fact]
    public async Task Create_rejects_a_blank_guest_name()
    {
        var result = await _service.CreateAsync(Request("101", 1, 2, "  "));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Validation, result.Error!.Kind);
    }

    [Fact]
    public async Task Cancel_frees_the_room_for_the_same_dates()
    {
        var created = await _service.CreateAsync(Request("101", 1, 4));
        await _service.CancelAsync(created.Value!.Id);

        var result = await _service.CreateAsync(Request("101", 1, 4, "Grace Hopper"));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Cancel_twice_is_a_validation_error()
    {
        var created = await _service.CreateAsync(Request("101", 1, 4));
        await _service.CancelAsync(created.Value!.Id);

        var result = await _service.CancelAsync(created.Value.Id);

        Assert.Equal(ErrorKind.Validation, result.Error!.Kind);
    }

    [Fact]
    public async Task Get_returns_not_found_for_an_unknown_id()
    {
        var result = await _service.GetAsync(Guid.NewGuid());

        Assert.Equal(ErrorKind.NotFound, result.Error!.Kind);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
