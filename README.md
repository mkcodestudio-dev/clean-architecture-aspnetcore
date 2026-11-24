# Clean architecture in ASP.NET Core

[![CI](https://github.com/mkcodestudio-dev/clean-architecture-aspnetcore/actions/workflows/ci.yml/badge.svg)](https://github.com/mkcodestudio-dev/clean-architecture-aspnetcore/actions/workflows/ci.yml)

A small room-booking API built to show how I structure ASP.NET Core projects: four layers, rules in the domain, no mediator or mapping libraries, and tests that run without a database or a web server.

It targets .NET 10. I wrote about the thinking behind it in [Clean architecture in ASP.NET Core without the ceremony](https://dev.to/mohammad_khalifeh/clean-architecture-in-aspnet-core-without-the-ceremony-51).

## Layout

```
src/
  Booking.Domain          entities and rules, no dependencies
  Booking.Application     use cases, repository interface, result type
  Booking.Infrastructure  in-memory repository
  Booking.Api             minimal API endpoints
tests/
  Booking.Application.Tests
```

Dependencies point inward: the API and infrastructure reference the application layer, which references the domain. Nothing in the domain or application layer knows about ASP.NET Core.

## Rules the domain enforces

- Check-out must be after check-in.
- Check-in cannot be in the past.
- A room cannot be booked twice for overlapping nights. A guest can check in on the day another checks out.
- A cancelled reservation frees the room and cannot be cancelled again.

## Design choices

- **Expected failures return a result** (validation, conflict, not found) instead of throwing. Exceptions are reserved for broken invariants inside the domain.
- **The repository checks availability and saves in one call** (`TryAddAsync`), so two requests cannot both take the same room. With a real database this becomes a transaction or a unique constraint.
- **Time comes from `TimeProvider`**, so tests control the date.
- **No MediatR or AutoMapper.** One service class and one small mapping method are enough here.

## Run it

```
dotnet run --project src/Booking.Api
```

```
curl -X POST http://localhost:5000/reservations \
  -H "Content-Type: application/json" \
  -d '{"roomId":"101","guestName":"Ada Lovelace","checkIn":"2027-01-10","checkOut":"2027-01-13"}'
```

The port is printed when the app starts. Endpoints: `POST /reservations`, `GET /reservations/{id}`, `DELETE /reservations/{id}` (cancels), `GET /health`.

## Test it

```
dotnet test
```

## Author

Mohammad Khalifeh. Portfolio and articles: [mkcodestudio.com](https://mkcodestudio.com).

## License

MIT
