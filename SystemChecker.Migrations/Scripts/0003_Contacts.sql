CREATE TABLE tblContactType (
	ID INT IDENTITY(1,1) PRIMARY KEY,
    Name VARCHAR(255)
)

ALTER TABLE dbo.tblCheckNotificationTypeOption ADD Multiple BIT NOT NULL CONSTRAINT DEFAULT_TEMP DEFAULT 0
ALTER TABLE dbo.tblCheckNotificationTypeOption DROP CONSTRAINT DEFAULT_TEMP

ALTER TABLE dbo.tblCheckTypeOption ADD Multiple BIT NOT NULL CONSTRAINT DEFAULT_TEMP DEFAULT 0
ALTER TABLE dbo.tblCheckTypeOption DROP CONSTRAINT DEFAULT_TEMP

ALTER TABLE dbo.tblSubCheckTypeOption ADD Multiple BIT NOT NULL CONSTRAINT DEFAULT_TEMP DEFAULT 0
ALTER TABLE dbo.tblSubCheckTypeOption DROP CONSTRAINT DEFAULT_TEMP
GO

CREATE TABLE tblContact (
	ID INT IDENTITY(1,1) PRIMARY KEY,
    TypeID INT NOT NULL,
	Name VARCHAR(255) NOT NULL,
	Value VARCHAR(255) NOT NULL,
	CONSTRAINT FK_tblContact_tblContactType FOREIGN KEY (TypeID) REFERENCES dbo.tblContactType
)

SET IDENTITY_INSERT dbo.tblCheckNotificationType ON 
INSERT INTO dbo.tblCheckNotificationType
(
	ID,
	Name
)
VALUES (
2, -- ID - int
'Email' -- Name - varchar(255)
	    )
SET IDENTITY_INSERT dbo.tblCheckNotificationType OFF

SET IDENTITY_INSERT dbo.tblCheckNotificationType ON 
INSERT INTO dbo.tblCheckNotificationType
(
	ID,
	Name
)
VALUES (
3, -- ID - int
'SMS' -- Name - varchar(255)
	    )
SET IDENTITY_INSERT dbo.tblCheckNotificationType OFF

SET IDENTITY_INSERT dbo.tblCheckNotificationTypeOption ON
INSERT INTO dbo.tblCheckNotificationTypeOption
(
	ID,
	CheckNotificationTypeID,
	OptionTypeID,
	Label,
	DefaultValue,
	IsRequired,
    Multiple
)
VALUES
(   2,   -- ID - int
	2,   -- CheckNotificationTypeID - int
	10,   -- OptionTypeID - int
	'Email Addresses',  -- Label - varchar(255)
	NULL,  -- DefaultValue - varchar(1)
	1, -- IsRequired - bit
	1 -- Multiple - bit
)
SET IDENTITY_INSERT dbo.tblCheckNotificationTypeOption OFF

SET IDENTITY_INSERT dbo.tblCheckNotificationTypeOption ON
INSERT INTO dbo.tblCheckNotificationTypeOption
(
	ID,
	CheckNotificationTypeID,
	OptionTypeID,
	Label,
	DefaultValue,
	IsRequired,
    Multiple
)
VALUES
(   3,   -- ID - int
	3,   -- CheckNotificationTypeID - int
	11,   -- OptionTypeID - int
	'Phone Numbers',  -- Label - varchar(255)
	NULL,  -- DefaultValue - varchar(1)
	1, -- IsRequired - bit
	1 -- Multiple - bit
)
SET IDENTITY_INSERT dbo.tblCheckNotificationTypeOption OFF

SET IDENTITY_INSERT dbo.tblContactType ON
INSERT INTO dbo.tblContactType
(
	ID,
	Name
)
VALUES (1, -- ID - int
	'Email' -- Name - varchar(255)
	    )
SET IDENTITY_INSERT dbo.tblContactType OFF

SET IDENTITY_INSERT dbo.tblContactType ON
INSERT INTO dbo.tblContactType
(
	ID,
	Name
)
VALUES (2, -- ID - int
	'Phone' -- Name - varchar(255)
	    )
SET IDENTITY_INSERT dbo.tblContactType OFF