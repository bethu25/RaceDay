CREATE DATABASE RaceDay;
USE RaceDay;

CREATE TABLE Users
(
    UserID INT PRIMARY KEY IDENTITY(1,1),
    FullName VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    Password VARCHAR(100) NOT NULL,
    Role VARCHAR(20) NOT NULL,
    PhoneNumber VARCHAR(20) NOT NULL
);

CREATE TABLE Events
(
    EventID INT PRIMARY KEY IDENTITY(1,1),
    OrganiserID INT NOT NULL,
    EventName VARCHAR(100) NOT NULL,
    EventDate DATE NOT NULL,
    Location VARCHAR(150) NOT NULL,
    Description VARCHAR(500),

    FOREIGN KEY (OrganiserID) REFERENCES Users(UserID)
);

CREATE TABLE Categories
(
    CategoryID INT PRIMARY KEY IDENTITY(1,1),
    EventID INT NOT NULL,
    CategoryName VARCHAR(100) NOT NULL,
    Distance DECIMAL(10,2) NOT NULL,
    EntryFee DECIMAL(10,2) NOT NULL,
    MaxParticipants INT NOT NULL,

    FOREIGN KEY (EventID) REFERENCES Events(EventID)
);


CREATE TABLE Enrolments
(
    EnrolmentID INT PRIMARY KEY IDENTITY(1,1),
    ParticipantID INT NOT NULL,
    CategoryID INT NOT NULL,
    EnrolmentDate DATE NOT NULL DEFAULT GETDATE(),
    Status VARCHAR(20) NOT NULL DEFAULT 'Active',

    FOREIGN KEY (ParticipantID) REFERENCES Users(UserID),
    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID)
);

INSERT INTO Users (FullName, Email, Password, Role, PhoneNumber)
VALUES
('John Smith', 'john@raceday.com', 'Password123', 'Organiser', '0712345678'),
('Sarah Jones', 'sarah@raceday.com', 'Password123', 'Organiser', '0723456789'),
('Mike Brown', 'mike@raceday.com', 'Password123', 'Participant', '0734567890'),
('Lisa Williams', 'lisa@raceday.com', 'Password123', 'Participant', '0745678901');

INSERT INTO Events (OrganiserID, EventName, EventDate, Location, Description)
VALUES
(1, 'Cape Town City Run', '2026-10-10', 'Cape Town', 'A road running event through Cape Town.'),
(1, 'Johannesburg Fun Run', '2026-11-15', 'Johannesburg', 'A community fun run for all participants.'),
(2, 'Durban Coastal Race', '2026-12-05', 'Durban', 'A coastal road race along the Durban beachfront.');

INSERT INTO Categories (EventID, CategoryName, Distance, EntryFee, MaxParticipants)
VALUES
(1, '5KM Run', 5.00, 100.00, 200),
(1, '10KM Run', 10.00, 150.00, 300),
(2, '5KM Fun Run', 5.00, 80.00, 250),
(2, '10KM Run', 10.00, 130.00, 300),
(3, '10KM Coastal Run', 10.00, 150.00, 300),
(3, '21KM Half Marathon', 21.10, 200.00, 400);

INSERT INTO Enrolments (ParticipantID, CategoryID)
VALUES
(3, 1),
(3, 4),
(4, 2),
(4, 5);

INSERT INTO Results (EnrolmentID, FinishTime, Position)
VALUES
(1, '00:28:35', 12),
(2, '00:55:20', 25),
(3, '00:31:10', 18);

INSERT INTO Routes (EventID, RouteName, Distance, RouteDescription)
VALUES
(1, 'Cape Town City Route', 10.00, 'Route through the main streets of Cape Town.'),
(2, 'Johannesburg Park Route', 10.00, 'Route through local parks and surrounding streets.'),
(3, 'Durban Coastal Route', 21.10, 'Route along the Durban beachfront and coastal roads.');

USE RaceDay;

SELECT * FROM Users;
SELECT * FROM Events;
SELECT * FROM Categories;
SELECT * FROM Enrolments;
SELECT * FROM Results;
SELECT * FROM Routes;
