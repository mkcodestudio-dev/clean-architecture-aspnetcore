using Booking.Application;
using Booking.Domain;

namespace Booking.Infrastructure;

public sealed class InMemoryReservationRepository : IReservationRepository
{
    private readonly object _gate = new();
    private readonly Dictionary<Guid, Reservation> _items = new();

    public Task<bool> TryAddAsync(Reservation reservation, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            if (_items.Values.Any(existing => existing.Blocks(reservation.RoomId, reservation.Stay)))
            {
                return Task.FromResult(false);
            }

            _items[reservation.Id] = reservation;
            return Task.FromResult(true);
        }
    }

    public Task<Reservation?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            return Task.FromResult(_items.GetValueOrDefault(id));
        }
    }

    public Task UpdateAsync(Reservation reservation, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            _items[reservation.Id] = reservation;
        }

        return Task.CompletedTask;
    }
}
