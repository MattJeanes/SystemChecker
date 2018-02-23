INSERT INTO dbo.tblGlobalSettings ([Key],Value)
VALUES
('ResultRetentionMonths','12'),
('ResultAggregateDays','30'),
('CleanupSchedule','"0 0 6 * * ?"'),
('LoginExpireAfterDays','30')