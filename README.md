# RaceDay

## 1. System Description

RaceDay is a web-based event management system designed for road running, walking and cycling events in South Africa.

The system provides features for event organisers to manage sporting events and for participants to register, enrol in race categories and view their results. It aims to simplify event administration and provide participants with a convenient way to manage their race activities.

The RaceDay project includes a SQL Server database and an ASP.NET Core Web API developed using C# and .NET 8.

## 2. User Roles

### 2.1 Organiser

The Organiser manages sporting events on the RaceDay system.

Organisers can:

* Create and update events.
* Delete events.
* Create and manage event categories.
* View participant enrolments.
* Record and update participant results.
* Manage route information.
* View results associated with their own events.

### 2.2 Participant

The Participant takes part in events managed through RaceDay.

Participants can:

* Register an account.
* Log in to the system.
* View and update their profile.
* Browse available events.
* Enrol in event categories.
* View their enrolments.
* View their own race results.

## 3. Technologies Used

The following technologies and tools are used in the project:

* **Programming language:** C#
* **Framework:** ASP.NET Core Web API
* **.NET version:** .NET 8
* **Database:** Microsoft SQL Server
* **Object-relational mapper:** Entity Framework Core
* **API documentation and testing:** Swagger / OpenAPI
* **Unit testing:** xUnit
* **Version control:** Git and GitHub
* **CI/CD:** GitHub Actions
* **Development environment:** Microsoft Visual Studio

## 4. Database

RaceDay uses Microsoft SQL Server to store and manage application data.

The database contains the following main entities:

* Users
* Events
* Categories
* Enrolments
* Results
* Routes

Entity Framework Core is used to interact with the database from the API.

The database design and SQL script are available in the `docs` folder.

### Database Screenshot

The following screenshot shows the RaceDay database in SQL Server Management Studio.

![RaceDay Database](https://github.com/bethu25/RaceDay/blob/6afd69a6777242ec6c6061f471b779e2f8a927ad/Screenshot%202026-09-04%20125751.png)

## 5. Web API

The RaceDay Web API is developed using ASP.NET Core and C#.

The API provides endpoints for managing application functionality, including:

* User registration and login.
* User profile management.
* Event management.
* Category management.
* Participant enrolments.
* Race results.
* Route information.

The API uses session-based authentication and role-based authorization to control access to protected operations.

The API also includes endpoints for testing database connectivity and session-related functionality.

### API Documentation

Swagger / OpenAPI can be used to inspect and test the API endpoints when the application is running.

To access Swagger, run the API project in Visual Studio and open the Swagger UI address configured for your development environment.

The API endpoint plan is available in:

`docs/API-Endpoint-Plan.md`

## 6. Unit Testing

The project includes a separate `RaceDay.API.Tests` project for automated unit testing.

The tests cover important application behaviours, including:

* Authentication and registration.
* Event and category management.
* Route management.
* Participant enrolments.
* Result creation and updates.
* Profile retrieval and updates.
* Session-related behaviour.
* Database test endpoints.

The tests use xUnit and Entity Framework Core InMemory to test application logic without requiring a separate SQL Server database for every test.

The tests can be executed locally using the following command from the solution directory:

```bash
dotnet test --configuration Release
```

## 7. Continuous Integration and Continuous Deployment (CI/CD)

RaceDay uses GitHub Actions to automate the build and testing process.

The workflow is defined in:

`.github/workflows/dotnet.yml`

The workflow is configured to run when changes are pushed to the `main` branch, when pull requests target `main`, or when the workflow is started manually.

The pipeline performs the following steps:

1. Checks out the repository.
2. Sets up the .NET 8 SDK.
3. Restores the required NuGet packages.
4. Builds the solution in Release configuration.
5. Runs the automated unit tests.

This process helps identify build errors and failing tests before changes are accepted.

### Successful CI/CD Build

The following screenshot shows a successful CI/CD build from the RaceDay repository.

![Successful RaceDay CI/CD Build](https://github.com/bethu25/RaceDay/blob/6f2b59d2f1ea439856d474d3b8f62078180faf4e/Screenshot%202026-10-16%20204607.png)

**Note:** Update this screenshot if necessary so that it shows the latest successful GitHub Actions run, including the build and unit-test steps.

## 8. Project Structure

The main components of the RaceDay solution include:

| Component           | Description                                                  |
| ------------------- | ------------------------------------------------------------ |
| `RaceDay.API.Clean` | ASP.NET Core Web API project                                 |
| `RaceDay.API.Tests` | Automated unit tests                                         |
| `docs`              | Project documentation, database script and API endpoint plan |
| `.github/workflows` | GitHub Actions workflow configuration                        |

The solution is organised into controllers, models, data access components, DTOs and middleware to separate the responsibilities of different parts of the application.

## 9. Running the Project

### Prerequisites

Before running RaceDay, ensure that the following are installed:

* Microsoft Visual Studio with the ASP.NET and web development workload.
* .NET 8 SDK.
* Microsoft SQL Server.
* SQL Server Management Studio (SSMS).

### Setup Instructions

1. Clone or download the RaceDay repository from GitHub.
2. Open the solution file in Visual Studio.
3. Configure the database connection string in the appropriate application configuration file.
4. Ensure that the SQL Server database is available and configured for the application.
5. Restore the required NuGet packages.
6. Build the solution.
7. Set `RaceDay.API.Clean` as the startup project.
8. Run the application.
9. Open Swagger UI using the address configured for the running application to inspect and test the available API endpoints.

To restore packages from the solution directory, run:

```bash
dotnet restore
```

To build the solution, run:

```bash
dotnet build --configuration Release
```

To run the unit tests, run:

```bash
dotnet test --configuration Release
```

## 10. Version Control

Git and GitHub are used to manage the RaceDay source code and track changes throughout development.

The repository contains the application source code, database documentation, automated tests and CI/CD workflow.

Contributions and updates are tracked through commits to maintain a record of the project's development.
