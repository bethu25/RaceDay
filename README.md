# RaceDay

## System Description

RaceDay is a web-based event management system designed for road running, walking and cycling events in South Africa.

The system allows event organisers to create and manage events, categories and participant results. Participants can browse available events, enrol in race categories and view their results.

The system is designed to make it easier for organisers to manage events and for participants to enter and track their performance.

## User Roles

### Organiser

The Organiser is responsible for managing RaceDay events. Organisers can:

- Create new events
- Update event information
- Delete events
- Create and manage event categories
- View participant enrolments
- Record participant results
- Manage route information

### Participant

The Participant is a user who takes part in RaceDay events. Participants can:

- Create an account
- Log into the system
- Manage their profile
- Browse available events
- Enrol in event categories
- View their enrolments
- View their race results

## Database

The RaceDay system uses a SQL Server database to store information about:

- Users
- Events
- Categories
- Enrolments
- Results
- Routes

The database design and SQL script are available in the `docs` folder.

## MY Database 
![image alt](https://github.com/bethu25/RaceDay/blob/6afd69a6777242ec6c6061f471b779e2f8a927ad/Screenshot%202026-09-04%20125751.png)

 
## API

The planned RaceDay API provides endpoints for authentication, user profiles, events, categories, enrolments and results.

The API endpoint plan is available in:

`docs/API-Endpoint-Plan.md`

## CI/CD

The RaceDay project uses GitHub Actions for Continuous Integration and Continuous Deployment (CI/CD).

The CI/CD pipeline is used to automatically build and test the project when changes are pushed to the GitHub repository.

### Successful Build(TEST)

The following screenshot shows a successful green CI/CD build:

![image alt](https://github.com/bethu25/RaceDay/blob/abf48e9c69a688d57bbc38baaf9d69e57d2e925c/Screenshot%202026-09-04%20200011.png)

 
