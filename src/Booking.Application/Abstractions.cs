using Booking.Domain;

namespace Booking.Application;

public interface IReservationRepository
{
    Task<bool> TryAddAsync(Reservation reservation, CancellationToken cancellationToken);

    Task<Reservation?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task UpdateAsync(Reservation reservation, CancellationToken cancellationToken);
}

public enum ErrorKind
{
    Validation,
    Conflict,
    NotFound
}

public sealed record Error(ErrorKind Kind, string Message);

public sealed class Result<T>
{
    private Result(T? value, Error? error)
    {
        Value = value;
        Error = error;
    }

    public T? Value { get; }

    public Error? Error { get; }

    public bool IsSuccess => Error is null;

    public static Result<T> Success(T value) => new(value, null);

    public static Result<T> Failure(Error error) => new(default, error);
}

public sealed record CreateReservationRequest(
    string RoomId,
    string GuestName,
    DateOnly CheckIn,
    DateOnly CheckOut);

public sealed record ReservationDto(
    Guid Id,
    string RoomId,
    string GuestName,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int Nights,
    string Status)
{
    public static ReservationDto From(Reservation reservation) => new(
        reservation.Id,
        reservation.RoomId,
        reservation.GuestName,
        reservation.Stay.CheckIn,
        reservation.Stay.CheckOut,
        reservation.Stay.Nights,
        reservation.Status.ToString());
}
