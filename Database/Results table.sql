USE RaceDay;

CREATE TABLE Results
(
    ResultID INT PRIMARY KEY IDENTITY(1,1),
    EnrolmentID INT NOT NULL UNIQUE,
    FinishTime TIME,
    Position INT,

    FOREIGN KEY (EnrolmentID) REFERENCES Enrolments(EnrolmentID)
);