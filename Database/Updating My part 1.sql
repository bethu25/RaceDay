-- Select the existing RaceDay database.
USE RaceDay;
GO

-- Add the primary advertised distance and event type
-- to the existing Events table.
ALTER TABLE Events
ADD
    Distance DECIMAL(10,2) NULL,
    EventType VARCHAR(10) NULL;
GO

-- Set the primary advertised distance for each existing event.
-- These values represent the main event distance, not every category.
UPDATE Events
SET Distance = 10.00,
    EventType = 'Run'
WHERE EventID = 1;

UPDATE Events
SET Distance = 10.00,
    EventType = 'Run'
WHERE EventID = 2;

UPDATE Events
SET Distance = 21.10,
    EventType = 'Run'
WHERE EventID = 3;
GO

-- Check that the new columns contain the expected values.
SELECT
    EventID,
    EventName,
    EventDate,
    Location,
    Distance,
    EventType
FROM Events;
GO

-- Ensure every event must have a distance and event type.
ALTER TABLE Events
ALTER COLUMN Distance DECIMAL(10,2) NOT NULL;
GO

ALTER TABLE Events
ALTER COLUMN EventType VARCHAR(10) NOT NULL;
GO

-- Restrict event types to the three types allowed by the assignment.
ALTER TABLE Events
ADD CONSTRAINT CK_Events_EventType
CHECK (EventType IN ('Run', 'Walk', 'Cycle'));
GO