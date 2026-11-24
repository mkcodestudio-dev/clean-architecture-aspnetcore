namespace Booking.Domain;

/// <summary>Raised when a business rule is broken.</summary>
public sealed class DomainException(string message) : Exception(message);

/// <summary>
/// A stay from check-in (inclusive) to check-out (exclusive).
/// Two stays that touch (one checks out the day the next checks in) do not overlap.
/// </summary>
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
