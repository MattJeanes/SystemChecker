﻿IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='tblSubCheckTypeOption' AND COLUMN_NAME='Multiple') BEGIN
	ALTER TABLE dbo.tblSubCheckTypeOption ADD Multiple BIT NOT NULL CONSTRAINT DEFAULT_TEMP DEFAULT 0
	ALTER TABLE dbo.tblSubCheckTypeOption DROP CONSTRAINT DEFAULT_TEMP
END