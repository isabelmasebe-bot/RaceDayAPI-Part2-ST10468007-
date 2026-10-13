# RaceDay API - PROG6221 Part 2

RESTful API for the RaceDay event management system built with ASP.NET Core 8, Entity Framework Core, and Session-based authentication.

## Features

- User registration & login (Organiser / Participant)
- Role-based access control
- Event management (CRUD)
- Categories and event-category linking
- Participant enrolments
- Results / Leaderboard
- Full unit test coverage
- Swagger documentation
- CI/CD with GitHub Actions

## How to run

1. Clone the repository
2. Open in Visual Studio 2022
3. Update the connection string in `appsettings.json` if needed
4. Run `Update-Database` in Package Manager Console
5. Press F5

## Swagger

Once running, navigate to:  
`https://localhost:xxxx/swagger`

## Unit Tests

All tests can be run from Test Explorer or with:

```bash
dotnet test
