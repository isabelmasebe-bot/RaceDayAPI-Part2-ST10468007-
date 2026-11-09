# RaceDay – Programming Part 3
### Automation of Web Application

## 1. Project Overview

RaceDay is a web-based race event management platform developed using ASP.NET Core MVC and a RESTful API.

The application allows Organisers to create and manage running events while Participants can browse available events, register for races and view their race results.

Programming Part 3 focuses on developing the MVC front end, integrating it with the RESTful API developed in Part 2, implementing authentication and role-based navigation, integrating Azure Blob Storage and containerising the application using Docker.

## 2. Technologies Used

The following technologies are used in the RaceDay project:

- **C#** – Main programming language.
- **ASP.NET Core MVC (.NET 8)** – Web application framework.
- **ASP.NET Core Web API** – Backend API for data operations.
- **HTML and CSS** – Front-end structure and styling.
- **Bootstrap** – Responsive user interface.
- **JavaScript** – Client-side functionality.
- **SQL Server** – Database management.
- **Azure Blob Storage** – Cloud-based image storage.
- **Docker** – Application containerisation.
- **Git and GitHub** – Source code management and version control.
- **GitHub Actions** – Continuous integration.

## 3. Project Structure

The repository is organised into the following main folders:

```text
RaceDayAPI/
│
├── RaceDayAPI/
│   ├── Controllers/
│   ├── Models/
│   ├── Data/
│   ├── Services/
│   └── Properties/
│
├── RaceDayMVC/
│   ├── Controllers/
│   ├── Models/
│   ├── Views/
│   ├── wwwroot/
│   ├── Properties/
│   ├── Dockerfile
│   ├── .dockerignore
│   └── appsettings.json
│
├── RaceDayAPI.Tests/
│
├── .github/
│   └── workflows/
│
└── README.md
```

## 4. Application Architecture

RaceDay uses the Model-View-Controller (MVC) architecture.

- **Model:** Represents application data and structures.
- **View:** Displays information and provides the user interface.
- **Controller:** Handles user requests and communicates with the RESTful API.

The MVC application consumes the Part 2 API for data operations.

The MVC project does not directly access the database or duplicate the API's business logic.

This separation improves maintainability and keeps the application organised.

## 5. Main Application Features

### 5.1 Authentication and Role-Based Navigation

The application provides:

- User registration.
- User login.
- Session-based authentication.
- Role-based navigation.
- Separate Organiser and Participant interfaces.
- Protected pages for authenticated users.
- Logout functionality and session termination.

### 5.2 Organiser Interface

Organisers have access to the following functionality:

- Dashboard displaying event information.
- Create, edit and delete events.
- Upload event banner images.
- Manage event categories.
- View Participant enrolments.
- View enrolment statuses.
- Capture and publish race results.

### 5.3 Participant Interface

Participants can:

- Browse upcoming race events.
- Filter available events.
- View event details and categories.
- Enrol in events.
- Select event categories.
- View their enrolled events.
- Check enrolment statuses.
- View personal race history and results.
- View and update profile information.
- Upload profile pictures.

### 5.4 Azure Blob Storage

Azure Blob Storage is integrated into the Part 2 API to support cloud-based image storage.

The application supports:

- Event banner image uploads.
- Participant profile picture uploads.
- Storing images in Azure Blob Storage.
- Retrieving and displaying uploaded images through the API.

The MVC application communicates with the API for all image operations and does not communicate directly with Azure Blob Storage.

## 6. Installation and Setup

### Prerequisites

Before running the application, ensure that you have installed:

1. Visual Studio 2022 or a compatible IDE.
2. .NET 8 SDK.
3. SQL Server and the required database configuration.
4. Docker Desktop.
5. Git.

### Step 1: Clone the Repository

Open a terminal and run:

```bash
git clone YOUR_GITHUB_REPOSITORY_URL
```

Navigate into the project directory:

```bash
cd RaceDayAPI
```

### Step 2: Configure the API

Open the API project and check its configuration files.

Ensure that the database connection string and Azure Blob Storage settings are configured correctly.

Do not commit passwords, connection secrets or Azure storage account keys to GitHub.

### Step 3: Run the API

From the repository root, run:

```bash
dotnet run --project ./RaceDayAPI/RaceDayAPI.csproj
```

The API's HTTP development address is:

```text
http://localhost:5059
```

### Step 4: Run the MVC Application

Open a second terminal and run:

```bash
dotnet run --project ./RaceDayMVC/RaceDayMVC.csproj
```

The MVC application is available at:

```text
http://localhost:5078
```

The MVC application must be able to communicate with the running API.

## 7. Docker Containerisation

Docker is used to package the RaceDay MVC application into a container.

The Dockerfile uses a multi-stage build:

1. The build stage uses the .NET SDK image to restore dependencies and publish the application.
2. The runtime stage uses the ASP.NET Core runtime image to run the published application.

The container exposes port 8080.

### Build the Docker Image

Run the following command from the repository root:

```bash
docker build -t raceday-mvc -f ./RaceDayMVC/Dockerfile ./RaceDayMVC
```

### Run the Docker Container

First, ensure that the RaceDay API is running on the host machine at port 5059.

Then run the MVC container using:

```bash
docker run -d --name raceday-mvc -p 8080:8080 --add-host=host.docker.internal:host-gateway -e "ApiSettings__BaseUrl=http://host.docker.internal:5059/" raceday-mvc
```

### Access the Application

Open your browser and navigate to:

```text
http://localhost:8080
```

### Verify the Container

Check running containers:

```bash
docker ps
```

View application logs:

```bash
docker logs raceday-mvc
```

### Stop the Container

```bash
docker stop raceday-mvc
```

### Remove the Container

```bash
docker rm raceday-mvc
```

### Rebuild the Application

After making changes to the MVC source code, rebuild the image and recreate the container using the build and run commands above.

**Important:** The current development setup runs the API and its SQL Server LocalDB dependency on the Windows host. The MVC application runs inside Docker and communicates with the API through `host.docker.internal`. The API and database are not included in the MVC Docker image.

## 8. GitHub and Continuous Integration

GitHub is used to manage and track the development of the RaceDay application.

Part 3 development is maintained in a clearly identified branch:

```text
part-3
```

The project uses Git for version control and GitHub for source code hosting.

The GitHub Actions workflow is updated to include the MVC project build alongside the existing API build.

Meaningful commits are used to document development progress and changes.

A minimum of 20 meaningful commits is required for Part 3.

## 9. Testing

The application should be tested to verify the following:

- Users can register and log in.
- Authentication state is maintained across pages.
- Organisers and Participants have separate interfaces.
- Unauthenticated users cannot access protected pages.
- Organisers can manage events and categories.
- Participants can browse and enrol in events.
- Enrolment statuses are displayed correctly.
- Race results can be captured and displayed.
- Images can be uploaded and retrieved through Azure Blob Storage.
- The MVC application can communicate with the API.
- The Docker image builds successfully.
- The Docker container starts successfully.
- The containerised application behaves consistently with the locally


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
