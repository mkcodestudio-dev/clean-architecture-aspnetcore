using Booking.Application;
using Booking.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IReservationRepository, InMemoryReservationRepository>();
builder.Services.AddScoped<ReservationService>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

var reservations = app.MapGroup("/reservations");

reservations.MapPost("/", async (
    CreateReservationRequest request,
    ReservationService service,
    CancellationToken cancellationToken) =>
{
    var result = await service.CreateAsync(request, cancellationToken);

    return result.IsSuccess
        ? Results.Created($"/reservations/{result.Value!.Id}", result.Value)
        : ToProblem(result.Error!);
});

reservations.MapGet("/{id:guid}", async (
    Guid id,
    ReservationService service,
    CancellationToken cancellationToken) =>
{
    var result = await service.GetAsync(id, cancellationToken);

    return result.IsSuccess
        ? Results.Ok(result.Value)
        : ToProblem(result.Error!);
});

reservations.MapDelete("/{id:guid}", async (
    Guid id,
    ReservationService service,
    CancellationToken cancellationToken) =>
{
    var result = await service.CancelAsync(id, cancellationToken);

    return result.IsSuccess
        ? Results.NoContent()
        : ToProblem(result.Error!);
});

app.Run();

static IResult ToProblem(Error error) => error.Kind switch
{
    ErrorKind.NotFound => Results.Problem(
        detail: error.Message,
        statusCode: StatusCodes.Status404NotFound,
        title: "Not found"),
    ErrorKind.Conflict => Results.Problem(
        detail: error.Message,
        statusCode: StatusCodes.Status409Conflict,
        title: "Conflict"),
    _ => Results.Problem(
        detail: error.Message,
        statusCode: StatusCodes.Status400BadRequest,
        title: "Invalid request")
};
