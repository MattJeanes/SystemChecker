ALTER TABLE dbo.tblCheckResult ALTER COLUMN DTS DATETIMEOFFSET
ALTER TABLE dbo.tblCheckNotification ALTER COLUMN Sent DATETIMEOFFSET
INSERT INTO dbo.tblGlobalSettings ([Key],Value)
VALUES
('TimeZoneId','"UTC"')