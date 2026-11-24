using Booking.Domain;

namespace Booking.Application;

public sealed class ReservationService(IReservationRepository repository, TimeProvider timeProvider)
{
    public async Task<Result<ReservationDto>> CreateAsync(
        CreateReservationRequest request,
        CancellationToken cancellationToken = default)
    {
        Reservation reservation;

        try
        {
            var stay = new DateRange(request.CheckIn, request.CheckOut);
            reservation = Reservation.Create(request.RoomId, request.GuestName, stay, Today());
        }
        catch (DomainException ex)
        {
            return Result<ReservationDto>.Failure(new Error(ErrorKind.Validation, ex.Message));
        }

        var added = await repository.TryAddAsync(reservation, cancellationToken);

        return added
            ? Result<ReservationDto>.Success(ReservationDto.From(reservation))
            : Result<ReservationDto>.Failure(
                new Error(ErrorKind.Conflict, "The room is already booked for those dates."));
    }

    public async Task<Result<ReservationDto>> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var reservation = await repository.GetAsync(id, cancellationToken);

        return reservation is null
            ? Result<ReservationDto>.Failure(new Error(ErrorKind.NotFound, "Reservation not found."))
            : Result<ReservationDto>.Success(ReservationDto.From(reservation));
    }

    public async Task<Result<ReservationDto>> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var reservation = await repository.GetAsync(id, cancellationToken);

        if (reservation is null)
        {
            return Result<ReservationDto>.Failure(new Error(ErrorKind.NotFound, "Reservation not found."));
        }

        try
        {
            reservation.Cancel();
        }
        catch (DomainException ex)
        {
            return Result<ReservationDto>.Failure(new Error(ErrorKind.Validation, ex.Message));
        }

        await repository.UpdateAsync(reservation, cancellationToken);

        return Result<ReservationDto>.Success(ReservationDto.From(reservation));
    }

    private DateOnly Today() => DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
}
