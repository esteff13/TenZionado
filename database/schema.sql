-- SQL Server. Run once in an empty CampusEvents database.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
CREATE TABLE dbo.Users (
 UserId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
 FullName nvarchar(100) NOT NULL,
 Email nvarchar(254) COLLATE Latin1_General_100_CI_AS NOT NULL,
 CONSTRAINT UQ_Users_Email UNIQUE (Email),
 CONSTRAINT CK_Users_FullName CHECK (LEN(LTRIM(RTRIM(FullName))) BETWEEN 1 AND 100),
 CONSTRAINT CK_Users_Email CHECK (Email NOT LIKE '% %' AND Email LIKE '_%@univ.edu.ph'
   AND LEN(Email)-LEN(REPLACE(Email,'@',''))=1)
);
CREATE TABLE dbo.Events (
 EventId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Events PRIMARY KEY,
 Title nvarchar(120) NOT NULL,
 Description nvarchar(600) NOT NULL,
 Venue nvarchar(120) NOT NULL,
 StartsAt datetime2(0) NOT NULL,
 Capacity int NOT NULL,
 CONSTRAINT CK_Events_Title CHECK (LEN(LTRIM(RTRIM(Title)))>0),
 CONSTRAINT CK_Events_Venue CHECK (LEN(LTRIM(RTRIM(Venue)))>0),
 CONSTRAINT CK_Events_Capacity CHECK (Capacity BETWEEN 1 AND 10000)
);
CREATE TABLE dbo.Registrations (
 RegistrationId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Registrations PRIMARY KEY,
 UserId int NOT NULL,
 EventId int NOT NULL,
 RegisteredAt datetime2(0) NOT NULL CONSTRAINT DF_Registrations_Time DEFAULT SYSUTCDATETIME(),
 CONSTRAINT FK_Registrations_Users FOREIGN KEY(UserId) REFERENCES dbo.Users(UserId) ON DELETE NO ACTION ON UPDATE NO ACTION,
 CONSTRAINT FK_Registrations_Events FOREIGN KEY(EventId) REFERENCES dbo.Events(EventId) ON DELETE NO ACTION ON UPDATE NO ACTION,
 CONSTRAINT UQ_Registrations_User_Event UNIQUE NONCLUSTERED(UserId,EventId)
);
-- The unique nonclustered index starts with UserId and serves that FK lookup.
-- Explicit EventId-leading nonclustered index supports the other FK and seat counts.
CREATE NONCLUSTERED INDEX IX_Registrations_EventId ON dbo.Registrations(EventId) INCLUDE(UserId);
COMMIT TRANSACTION;
