-- Check Notification Type Option

SELECT * INTO #temp FROM dbo.tblCheckNotificationTypeOption

DROP TABLE dbo.tblCheckNotificationTypeOption

CREATE TABLE [dbo].[tblCheckNotificationTypeOption]
(
[ID] [VARCHAR] (50) NOT NULL,
[CheckNotificationTypeID] [int] NOT NULL,
[OptionTypeID] [int] NOT NULL,
[Label] [varchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[DefaultValue] [varchar] (1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[IsRequired] [bit] NOT NULL,
[Multiple] [bit] NOT NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblCheckNotificationTypeOption] ADD CONSTRAINT [PK_tblCheckNotificationTypeOption] PRIMARY KEY CLUSTERED ([ID],[CheckNotificationTypeID]) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblCheckNotificationTypeOption] ADD CONSTRAINT [FK_tblCheckNotificationTypeOption_tblCheckNotificationType] FOREIGN KEY ([CheckNotificationTypeID]) REFERENCES [dbo].[tblCheckNotificationType] ([ID])
GO


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
SELECT CONVERT(VARCHAR(50),ID),
       CheckNotificationTypeID,
       OptionTypeID,
       Label,
       DefaultValue,
       IsRequired,
       Multiple FROM #temp

DROP TABLE #temp

DECLARE @CheckNotificationTypeOptionMapping TABLE(OldID VARCHAR(50), ID VARCHAR(50)) 

INSERT INTO @CheckNotificationTypeOptionMapping (OldID,ID)
VALUES
('1','ChannelID'),
('2','EmailAddresses'),
('3','PhoneNumbers')

UPDATE o SET o.ID=m.ID FROM dbo.tblCheckNotificationTypeOption o
INNER JOIN @CheckNotificationTypeOptionMapping m ON m.OldID = o.ID


WHILE EXISTS (SELECT 1 FROM @CheckNotificationTypeOptionMapping) BEGIN
	DECLARE @OldID VARCHAR(50)
	DECLARE @ID VARCHAR(50) 
	SELECT TOP 1 @ID=ID, @OldID=OldID FROM @CheckNotificationTypeOptionMapping
	UPDATE n SET
		n.Options = REPLACE(n.Options, '"' + @OldID + '":', '"' + @ID + '":')
	FROM dbo.tblCheckNotification n 
	WHERE CHARINDEX('"' + @OldID + '":', n.Options) > 0
	DELETE FROM @CheckNotificationTypeOptionMapping WHERE OldID=@OldID AND ID=@ID
END

-- Check Type Option

SELECT * INTO #temp FROM dbo.tblCheckTypeOption

DROP TABLE dbo.tblCheckTypeOption

CREATE TABLE [dbo].[tblCheckTypeOption]
(
[ID] [VARCHAR] (50) NOT NULL,
[CheckTypeID] [int] NOT NULL,
[OptionTypeID] [int] NOT NULL,
[Label] [varchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[DefaultValue] [varchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[IsRequired] [bit] NOT NULL,
[Multiple] [bit] NOT NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblCheckTypeOption] ADD CONSTRAINT [PK_tblCheckTypeOption] PRIMARY KEY CLUSTERED ([ID], [CheckTypeID]) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblCheckTypeOption] ADD CONSTRAINT [FK_tblCheckTypeOption_tblCheckType] FOREIGN KEY ([CheckTypeID]) REFERENCES [dbo].[tblCheckType] ([ID])
GO



INSERT INTO dbo.tblCheckTypeOption
(
	ID,
    CheckTypeID,
    OptionTypeID,
    Label,
    DefaultValue,
    IsRequired,
    Multiple
)
SELECT CONVERT(VARCHAR(50),ID),
       CheckTypeID,
       OptionTypeID,
       Label,
       DefaultValue,
       IsRequired,
       Multiple FROM #temp

DROP TABLE #temp

DECLARE @CheckTypeOptionMapping TABLE(OldID VARCHAR(50), ID VARCHAR(50)) 

INSERT INTO @CheckTypeOptionMapping (OldID,ID)
VALUES
('5','RequestUrl'),
('6','Authentication'),
('7','ConnectionString'),
('8','Query'),
('9','TimeoutMS'),
('10','TimeWarnMS'),
('12','ServerAddress'),
('13','TimeoutMS'),
('14','HttpMethod')

UPDATE o SET o.ID=m.ID FROM dbo.tblCheckTypeOption o
INNER JOIN @CheckTypeOptionMapping m ON m.OldID = o.ID


WHILE EXISTS (SELECT 1 FROM @CheckTypeOptionMapping) BEGIN
	DECLARE @OldID VARCHAR(50)
	DECLARE @ID VARCHAR(50) 
	SELECT TOP 1 @ID=ID, @OldID=OldID FROM @CheckTypeOptionMapping
	UPDATE n SET
		n.TypeOptions = REPLACE(n.TypeOptions, '"' + @OldID + '":', '"' + @ID + '":')
	FROM dbo.tblCheckData n 
	WHERE CHARINDEX('"' + @OldID + '":', n.TypeOptions) > 0
	DELETE FROM @CheckTypeOptionMapping WHERE OldID=@OldID AND ID=@ID
END

-- SubCheck Type Option

SELECT * INTO #temp FROM dbo.tblSubCheckTypeOption

DROP TABLE dbo.tblSubCheckTypeOption

CREATE TABLE [dbo].[tblSubCheckTypeOption]
(
[ID] [VARCHAR] (50) NOT NULL,
[SubCheckTypeID] [int] NOT NULL,
[OptionTypeID] [int] NOT NULL,
[Label] [varchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[DefaultValue] [varchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[IsRequired] [bit] NOT NULL,
[Multiple] [bit] NOT NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblSubCheckTypeOption] ADD CONSTRAINT [PK_tblSubCheckTypeOption] PRIMARY KEY CLUSTERED ([ID], [SubCheckTypeID]) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblSubCheckTypeOption] ADD CONSTRAINT [FK_tblSubCheckTypeOption_tblSubCheckType] FOREIGN KEY ([SubCheckTypeID]) REFERENCES [dbo].[tblSubCheckType] ([ID])
GO




INSERT INTO dbo.tblSubCheckTypeOption
(
	ID,
    SubCheckTypeID,
    OptionTypeID,
    Label,
    DefaultValue,
    IsRequired,
    Multiple
)
SELECT CONVERT(VARCHAR(50),ID),
       SubCheckTypeID,
       OptionTypeID,
       Label,
       DefaultValue,
       IsRequired,
       Multiple FROM #temp

DROP TABLE #temp

DECLARE @SubCheckTypeOptionMapping TABLE(OldID VARCHAR(50), ID VARCHAR(50)) 

INSERT INTO @SubCheckTypeOptionMapping (OldID,ID)
VALUES
('1','Text'),
('2','FieldName'),
('3','Exists'),
('4','Value'),
('6','Value'),
('7','FieldName'),
('8','Exists'),
('9','FieldName'),
('10','Value'),
('11','Exists'),
('12','FieldName'),
('13','Value'),
('14','Exists'),
('15','FieldName'),
('16','Value'),
('17','Exists')

UPDATE o SET o.ID=m.ID FROM dbo.tblSubCheckTypeOption o
INNER JOIN @SubCheckTypeOptionMapping m ON m.OldID = o.ID


WHILE EXISTS (SELECT 1 FROM @SubCheckTypeOptionMapping) BEGIN
	DECLARE @OldID VARCHAR(50)
	DECLARE @ID VARCHAR(50) 
	SELECT TOP 1 @ID=ID, @OldID=OldID FROM @SubCheckTypeOptionMapping
	UPDATE n SET
		n.Options = REPLACE(n.Options, '"' + @OldID + '":', '"' + @ID + '":')
	FROM dbo.tblSubCheck n 
	WHERE CHARINDEX('"' + @OldID + '":', n.Options) > 0
	DELETE FROM @SubCheckTypeOptionMapping WHERE OldID=@OldID AND ID=@ID
END

-- SubCheck Type - Identifier

SELECT * INTO #temp FROM dbo.tblSubCheckType

ALTER TABLE dbo.tblSubCheck DROP CONSTRAINT FK_tblSubCheck_tblSubCheckType
ALTER TABLE dbo.tblSubCheckTypeOption DROP CONSTRAINT FK_tblSubCheckTypeOption_tblSubCheckType

DROP TABLE dbo.tblSubCheckType

CREATE TABLE [dbo].[tblSubCheckType]
(
[ID] [int] NOT NULL IDENTITY(1, 1),
[CheckTypeID] [int] NOT NULL,
[Identifier] [VARCHAR] (50) NULL,
[Name] [varchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblSubCheckType] ADD CONSTRAINT [PK_tblSubCheckType] PRIMARY KEY CLUSTERED ([ID]) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblSubCheckType] ADD CONSTRAINT [FK_tblSubCheckType_tblCheckType] FOREIGN KEY ([CheckTypeID]) REFERENCES [dbo].[tblCheckType] ([ID])
GO

DECLARE @SubCheckTypeIdentifierMapping TABLE(ID INT NOT NULL, Identifier VARCHAR(50))

INSERT INTO @SubCheckTypeIdentifierMapping (ID,Identifier) VALUES
(1,'ResponseContains'),
(2,'JsonProperty'),
(3,'FieldEqualTo'),
(4,'FieldNotEqualTo'),
(5,'FieldGreaterThan'),
(6,'FieldLessThan')

SET IDENTITY_INSERT dbo.tblSubCheckType ON
INSERT INTO dbo.tblSubCheckType
(
	ID,
    CheckTypeID,
	Identifier,
    Name
)
SELECT t.ID,
       t.CheckTypeID,
	   m.Identifier,
       t.Name FROM #temp t INNER JOIN @SubCheckTypeIdentifierMapping m ON m.ID = t.ID


SET IDENTITY_INSERT dbo.tblSubCheckType OFF

DROP TABLE #temp

ALTER TABLE [dbo].[tblSubCheck] ADD CONSTRAINT [FK_tblSubCheck_tblSubCheckType] FOREIGN KEY ([TypeID]) REFERENCES [dbo].[tblSubCheckType] ([ID])
ALTER TABLE [dbo].[tblSubCheckTypeOption] ADD CONSTRAINT [FK_tblSubCheckTypeOption_tblSubCheckType] FOREIGN KEY ([SubCheckTypeID]) REFERENCES [dbo].[tblSubCheckType] ([ID])

-- Sort Order
ALTER TABLE dbo.tblCheckNotificationTypeOption ADD SortOrder INT NULL
ALTER TABLE dbo.tblCheckTypeOption ADD SortOrder INT NULL
ALTER TABLE dbo.tblSubCheckTypeOption ADD SortOrder INT NULL
GO

UPDATE dbo.tblSubCheckTypeOption SET SortOrder=0 WHERE ID='FieldName'
UPDATE dbo.tblSubCheckTypeOption SET SortOrder=1 WHERE ID='Value'
UPDATE dbo.tblSubCheckTypeOption SET SortOrder=2 WHERE ID='Exists'

DECLARE @SortOrderMapping TABLE (ID VARCHAR(50), CheckTypeID INT NOT NULL, SortOrder INT NULL)
INSERT INTO @SortOrderMapping (ID,CheckTypeID,SortOrder) VALUES
('RequestUrl',1,0),
('HttpMethod',1,1),
('Authentication',1,2),
('TimeoutMS',1,3),
('TimeWarnMS',1,4),
('ConnectionString',2,0),
('Query',2,1),
('ServerAddress',3,0),
('TimeoutMS',3,1)

UPDATE cto SET cto.SortOrder=m.SortOrder FROM dbo.tblCheckTypeOption cto INNER JOIN @SortOrderMapping m ON m.CheckTypeID = cto.CheckTypeID AND m.ID = cto.ID
