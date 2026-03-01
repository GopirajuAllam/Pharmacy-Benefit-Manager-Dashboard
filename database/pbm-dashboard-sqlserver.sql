IF OBJECT_ID('dbo.PharmacyClaims', 'U') IS NOT NULL DROP TABLE dbo.PharmacyClaims;
IF OBJECT_ID('dbo.PriorAuthorizationRequests', 'U') IS NOT NULL DROP TABLE dbo.PriorAuthorizationRequests;
IF OBJECT_ID('dbo.PbmFacilities', 'U') IS NOT NULL DROP TABLE dbo.PbmFacilities;
GO

CREATE TABLE dbo.PbmFacilities (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(150) NOT NULL,
    Region NVARCHAR(80) NOT NULL,
    LicensedBeds INT NOT NULL,
    ActivePatients INT NOT NULL
);
GO

CREATE TABLE dbo.PharmacyClaims (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PbmFacilityId INT NOT NULL,
    MedicationName NVARCHAR(120) NOT NULL,
    Department NVARCHAR(80) NOT NULL,
    FilledUtc DATETIME2 NOT NULL,
    BilledAmount DECIMAL(18,2) NOT NULL,
    PaidAmount DECIMAL(18,2) NOT NULL,
    IsGeneric BIT NOT NULL,
    IsSpecialty BIT NOT NULL,
    TurnaroundHours DECIMAL(9,2) NOT NULL,
    Status INT NOT NULL,
    CONSTRAINT FK_PharmacyClaims_Facility FOREIGN KEY (PbmFacilityId) REFERENCES dbo.PbmFacilities(Id)
);
GO

CREATE TABLE dbo.PriorAuthorizationRequests (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PbmFacilityId INT NOT NULL,
    MedicationName NVARCHAR(120) NOT NULL,
    RequestedUtc DATETIME2 NOT NULL,
    ReviewedUtc DATETIME2 NULL,
    Status INT NOT NULL,
    IsUrgent BIT NOT NULL,
    CONSTRAINT FK_PriorAuthorizationRequests_Facility FOREIGN KEY (PbmFacilityId) REFERENCES dbo.PbmFacilities(Id)
);
GO

INSERT INTO dbo.PbmFacilities (Name, Region, LicensedBeds, ActivePatients) VALUES
('North Ridge Medical Center', 'Midwest', 280, 1450),
('Starlight Community Hospital', 'South', 190, 980),
('Cedar Valley Hospital', 'West', 320, 1640);
GO

INSERT INTO dbo.PharmacyClaims (PbmFacilityId, MedicationName, Department, FilledUtc, BilledAmount, PaidAmount, IsGeneric, IsSpecialty, TurnaroundHours, Status) VALUES
(1, 'Metformin', 'Inpatient', DATEADD(HOUR, -2, SYSUTCDATETIME()), 240.00, 218.40, 1, 0, 4.2, 1),
(1, 'Ozempic', 'Ambulatory', DATEADD(DAY, -1, SYSUTCDATETIME()), 640.00, 537.60, 0, 1, 7.4, 1),
(2, 'Humira', 'Oncology', DATEADD(DAY, -2, SYSUTCDATETIME()), 880.00, 739.20, 0, 1, 8.6, 2),
(2, 'Rosuvastatin', 'Cardiology', DATEADD(DAY, -1, SYSUTCDATETIME()), 210.00, 191.10, 1, 0, 5.0, 1),
(3, 'Jardiance', 'Emergency', DATEADD(HOUR, -6, SYSUTCDATETIME()), 390.00, 354.90, 0, 0, 6.2, 1),
(3, 'Stelara', 'Oncology', DATEADD(DAY, -3, SYSUTCDATETIME()), 910.00, 764.40, 0, 1, 9.1, 3);
GO

INSERT INTO dbo.PriorAuthorizationRequests (PbmFacilityId, MedicationName, RequestedUtc, ReviewedUtc, Status, IsUrgent) VALUES
(1, 'Ozempic', DATEADD(HOUR, -10, SYSUTCDATETIME()), DATEADD(HOUR, -4, SYSUTCDATETIME()), 1, 0),
(1, 'Humira', DATEADD(HOUR, -5, SYSUTCDATETIME()), NULL, 3, 1),
(2, 'Stelara', DATEADD(DAY, -1, SYSUTCDATETIME()), DATEADD(HOUR, -8, SYSUTCDATETIME()), 2, 0),
(3, 'Jardiance', DATEADD(HOUR, -7, SYSUTCDATETIME()), NULL, 3, 1);
GO

CREATE OR ALTER VIEW dbo.vw_PbmDashboardSummary
AS
SELECT
    COUNT(DISTINCT f.Id) AS ActiveFacilities,
    SUM(f.ActivePatients) AS ActivePatients,
    SUM(CASE WHEN c.FilledUtc >= CAST(SYSUTCDATETIME() AS date) THEN 1 ELSE 0 END) AS ClaimsToday,
    SUM(CASE WHEN c.FilledUtc >= DATEFROMPARTS(YEAR(SYSUTCDATETIME()), MONTH(SYSUTCDATETIME()), 1) THEN c.PaidAmount ELSE 0 END) AS PaidAmountMonthToDate,
    CAST(100.0 * SUM(CASE WHEN c.IsGeneric = 1 THEN 1 ELSE 0 END) / NULLIF(COUNT(c.Id), 0) AS DECIMAL(5,1)) AS GenericDispenseRate,
    CAST(100.0 * SUM(CASE WHEN pa.Status = 1 THEN 1 ELSE 0 END) / NULLIF(COUNT(pa.Id), 0) AS DECIMAL(5,1)) AS PriorAuthorizationApprovalRate
FROM dbo.PbmFacilities f
LEFT JOIN dbo.PharmacyClaims c ON c.PbmFacilityId = f.Id
LEFT JOIN dbo.PriorAuthorizationRequests pa ON pa.PbmFacilityId = f.Id;
GO
