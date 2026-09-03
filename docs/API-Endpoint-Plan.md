# RaceDay API Endpoint Plan

This document describes the REST API endpoints planned for the RaceDay
event management system.


| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/auth/register | Creates a new RaceDay user account. | None | FullName, Email, Password, Role, PhoneNumber | 201 Created - user account created; 400 Bad Request - invalid information |
| POST | /api/auth/login | Allows a user to log into their RaceDay account. | None | Email, Password | 200 OK - login successful; 401 Unauthorized - invalid login details |
| GET | /api/users/profile | Gets the profile information of the logged-in user. | Any | None | 200 OK - user profile returned; 404 Not Found - user does not exist |
| PUT | /api/users/profile | Updates the profile information of the logged-in user. | Any | FullName, Email, Password, PhoneNumber | 200 OK - profile updated; 404 Not Found - user does not exist |
| GET | /api/events | Gets a list of upcoming RaceDay events. | None | None | 200 OK - list of events returned |
| GET | /api/events/{id} | Gets the details of a specific event. | None | None | 200 OK - event returned; 404 Not Found - event does not exist |
| POST | /api/events | Creates a new RaceDay event. | Organiser | EventName, EventDate, Location, Description | 201 Created - event created; 400 Bad Request - invalid information |
| PUT | /api/events/{id} | Updates an existing RaceDay event. | Organiser | EventName, EventDate, Location, Description | 200 OK - event updated; 404 Not Found - event does not exist |
| DELETE | /api/events/{id} | Deletes an existing RaceDay event. | Organiser | None | 204 No Content - event deleted; 404 Not Found - event does not exist |
| GET | /api/events/{eventId}/categories | Gets all categories for a specific event. | None | None | 200 OK - categories returned; 404 Not Found - event does not exist |
| POST | /api/events/{eventId}/categories | Creates a new category for an event. | Organiser | CategoryName, Distance, EntryFee, MaxParticipants | 201 Created - category created; 400 Bad Request - invalid information |
| PUT | /api/categories/{id} | Updates an existing event category. | Organiser | CategoryName, Distance, EntryFee, MaxParticipants | 200 OK - category updated; 404 Not Found - category does not exist |
| DELETE | /api/categories/{id} | Deletes an existing event category. | Organiser | None | 204 No Content - category deleted; 404 Not Found - category does not exist |
| POST | /api/enrolments | Allows a participant to enter an event category. | Participant | CategoryID | 201 Created - enrolment created; 404 Not Found - category does not exist; 409 Conflict - participant is already enrolled |
| GET | /api/enrolments/my | Gets all event enrolments belonging to the logged-in participant. | Participant | None | 200 OK - enrolments returned |
| GET | /api/events/{eventId}/enrolments | Gets all participant enrolments for a specific event. | Organiser | None | 200 OK - enrolments returned; 404 Not Found - event does not exist |
| POST | /api/results | Records a participant's result for an enrolment. | Organiser | EnrolmentID, FinishTime, Position | 201 Created - result recorded; 404 Not Found - enrolment does not exist; 400 Bad Request - invalid information |
| GET | /api/results/my | Gets all results belonging to the logged-in participant. | Participant | None | 200 OK - results returned |
| GET | /api/events/{eventId}/results | Gets the results for participants in a specific event. | Organiser | None | 200 OK - results returned; 404 Not Found - event does not exist |