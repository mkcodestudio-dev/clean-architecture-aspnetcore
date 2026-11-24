namespace Booking.Domain;

public enum ReservationStatus
{
    Confirmed,
    Cancelled
}

public sealed class Reservation
{
    private Reservation(Guid id, string roomId, string guestName, DateRange stay)
    {
        Id = id;
        RoomId = roomId;
        GuestName = guestName;
        Stay = stay;
        Status = ReservationStatus.Confirmed;
    }

    public Guid Id { get; }

    public string RoomId { get; }

    public string GuestName { get; }

    public DateRange Stay { get; }

    public ReservationStatus Status { get; private set; }

    public static Reservation Create(string roomId, string guestName, DateRange stay, DateOnly today)
    {
        if (string.IsNullOrWhiteSpace(roomId))
        {
            throw new DomainException("Room is required.");
        }

        if (string.IsNullOrWhiteSpace(guestName))
        {
            throw new DomainException("Guest name is required.");
        }

        if (stay.CheckIn < today)
        {
            throw new DomainException("Check-in cannot be in the past.");
        }

        return new Reservation(Guid.NewGuid(), roomId.Trim(), guestName.Trim(), stay);
    }

    public void Cancel()
    {
        if (Status == ReservationStatus.Cancelled)
        {
            throw new DomainException("Reservation is already cancelled.");
        }

        Status = ReservationStatus.Cancelled;
    }

    public bool Blocks(string roomId, DateRange stay) =>
        Status == ReservationStatus.Confirmed
        && RoomId == roomId
        && Stay.Overlaps(stay);
}
