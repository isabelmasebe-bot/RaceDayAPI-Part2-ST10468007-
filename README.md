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

## Docker (Part 3 - MVC application)

The MVC project is containerised separately from the Part 2 API. The MVC container
must be able to reach the API. The current API uses SQL Server LocalDB, so for this
project setup keep the API running on the Windows host and run the MVC application
in Docker.

### 1. Start the API on the host

Open PowerShell at the repository root and run:

```powershell
dotnet run --project .\RaceDayAPI\RaceDayAPI.csproj --urls "http://0.0.0.0:5059"
```

Keep this terminal open. Confirm the API responds at `http://localhost:5059/swagger`
(if Swagger is enabled for the current environment).

### 2. Build the MVC Docker image

From the repository root:

```powershell
docker build -t raceday-mvc -f .\RaceDayMVC\Dockerfile .\RaceDayMVC
```

### 3. Run the MVC container

```powershell
docker run -d --name raceday-mvc -p 8080:8080 --add-host=host.docker.internal:host-gateway -e "ApiSettings__BaseUrl=http://host.docker.internal:5059/" raceday-mvc
```

Open `http://localhost:8080` in your browser.

### Useful Docker commands

```powershell
docker ps
docker logs raceday-mvc
docker stop raceday-mvc
docker rm raceday-mvc
```

To rebuild after changing the MVC source:

```powershell
docker stop raceday-mvc
docker rm raceday-mvc
docker build -t raceday-mvc -f .\RaceDayMVC\Dockerfile .\RaceDayMVC
docker run -d --name raceday-mvc -p 8080:8080 --add-host=host.docker.internal:host-gateway -e "ApiSettings__BaseUrl=http://host.docker.internal:5059/" raceday-mvc
```

The `ApiSettings__BaseUrl` environment variable overrides `ApiSettings:BaseUrl`
in `RaceDayMVC/appsettings.json` without changing the local development setting.
The API must remain running on the host while the MVC container is being used.

> Note: This Docker setup containerises the MVC application, as required by Part 3.
> It does not containerise SQL Server LocalDB or the Part 2 API. The API remains on
> the host because LocalDB is Windows-specific and is currently used by the API's
> connection string. If the API/database deployment is changed later, update the
> API URL and deployment instructions accordingly.
