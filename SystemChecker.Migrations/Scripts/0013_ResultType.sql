CREATE TABLE dbo.tblResultType (
	ID INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	Name VARCHAR(50) NOT NULL,
)

INSERT INTO dbo.tblResultType (Name) VALUES
('Success'),
('Failed'),
('Warning')

CREATE TABLE dbo.tblResultStatus (
	ID INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	TypeID INT NOT NULL,
	Name VARCHAR(50) NOT NULL,
	CONSTRAINT FK_tblResultType_tblResultStatus FOREIGN KEY (TypeID) REFERENCES dbo.tblResultType (ID)
)

DECLARE @Success INT = (SELECT ID FROM dbo.tblResultType WHERE Name='Success')
DECLARE @Failed INT = (SELECT ID FROM dbo.tblResultType WHERE Name='Failed')
DECLARE @Warning INT = (SELECT ID FROM dbo.tblResultType WHERE Name='Warning')

INSERT INTO dbo.tblResultStatus (TypeID, Name) VALUES
(@Success,'Success'),
(@Failed,'Failed'),
(@Warning,'Warning'),
(@Warning,'Time Warning'),
(@Failed,'Timeout'),
(@Failed,'SubCheck Failed')


DECLARE @CheckResultMapping TABLE (Status INT NOT NULL, ResultStatus VARCHAR(50) NOT NULL)

INSERT INTO @CheckResultMapping (Status, ResultStatus) VALUES
(3,'Time Warning'),
(2,'Warning'),
(1,'Success'),
(-1,'Failed'),
(-2,'Timeout'),
(-3,'SubCheck Failed')

UPDATE cr SET cr.Status=rs.ID FROM dbo.tblCheckResult cr
INNER JOIN @CheckResultMapping m ON m.Status = cr.Status
INNER JOIN dbo.tblResultStatus rs ON rs.Name = m.ResultStatus

EXEC sp_rename 'dbo.tblCheckResult.Status', 'StatusID', 'COLUMN';
GO
ALTER TABLE dbo.tblCheckResult ADD CONSTRAINT FK_tblResultStatus_tblCheckResult FOREIGN KEY (StatusID) REFERENCES dbo.tblResultStatus (ID)


ALTER TABLE dbo.tblResultType ADD Identifier VARCHAR(50) NULL
GO

DECLARE @ResultTypeMapping TABLE (Name VARCHAR(50), Identifier VARCHAR(50))

INSERT INTO @ResultTypeMapping (Name, Identifier) VALUES
('Success', 'Success'),
('Failed', 'Failed'),
('Warning', 'Warning')

UPDATE rt SET rt.Identifier = m.Identifier FROM dbo.tblResultType rt INNER JOIN @ResultTypeMapping m ON m.Name = rt.Name

ALTER TABLE dbo.tblResultType ALTER COLUMN Identifier VARCHAR(50) NOT NULL

ALTER TABLE dbo.tblResultStatus ADD Identifier VARCHAR(50) NULL
GO

DECLARE @ResultStatusMapping TABLE (Name VARCHAR(50), Identifier VARCHAR(50))

INSERT INTO @ResultStatusMapping (Name, Identifier) VALUES
('Success', 'Success'),
('Failed', 'Failed'),
('Warning', 'Warning'),
('Time Warning', 'TimeWarning'),
('Timeout', 'Timeout'),
('SubCheck Failed', 'SubCheckFailed')

UPDATE rt SET rt.Identifier = m.Identifier FROM dbo.tblResultStatus rt INNER JOIN @ResultStatusMapping m ON m.Name = rt.Name

ALTER TABLE dbo.tblResultStatus ALTER COLUMN Identifier VARCHAR(50) NOT NULL