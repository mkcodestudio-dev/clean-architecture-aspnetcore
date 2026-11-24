namespace Booking.Domain;

public sealed class DomainException(string message) : Exception(message);

public sealed record DateRange
{
    public DateRange(DateOnly checkIn, DateOnly checkOut)
    {
        if (checkOut <= checkIn)
        {
            throw new DomainException("Check-out must be after check-in.");
        }

        CheckIn = checkIn;
        CheckOut = checkOut;
    }

    public DateOnly CheckIn { get; }

    public DateOnly CheckOut { get; }

    public int Nights => CheckOut.DayNumber - CheckIn.DayNumber;

    public bool Overlaps(DateRange other) =>
        CheckIn < other.CheckOut && other.CheckIn < CheckOut;
}
