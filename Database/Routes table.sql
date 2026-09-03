USE RaceDay;

CREATE TABLE Routes
(
    RouteID INT PRIMARY KEY IDENTITY(1,1),
    EventID INT NOT NULL UNIQUE,
    RouteName VARCHAR(100) NOT NULL,
    Distance DECIMAL(10,2) NOT NULL,
    RouteDescription VARCHAR(500),

    FOREIGN KEY (EventID) REFERENCES Events(EventID)
);