--------------------------------------------------------------------------------------------------------------------------
-- TABLES -----------------------------------------------------------------------------------------------------------------
--------------------------------------------------------------------------------------------------------------------------

CREATE TABLE [dbo].[tblCheck](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[Active] [bit] NOT NULL,
	[TypeID] [int] NOT NULL,
	[DataID] [int] NOT NULL,
CONSTRAINT [PK_tblCheck] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblCheckConnString]    Script Date: 12/12/2017 15:50:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE TABLE [dbo].[tblCheckConnString](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NULL,
	[Value] [varchar](512) NULL,
CONSTRAINT [PK_tblCheckConnString] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblCheckData]    Script Date: 12/12/2017 15:50:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE TABLE [dbo].[tblCheckData](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TypeOptions] [varchar](max) NOT NULL,
CONSTRAINT [PK_tblCheckData] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblCheckLogin]    Script Date: 12/12/2017 15:50:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE TABLE [dbo].[tblCheckLogin](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Username] [varchar](255) NOT NULL,
	[Password] [varchar](255) NOT NULL,
	[Domain] [varchar](255) NULL,
CONSTRAINT [PK_tblCheckLogin] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblCheckNotification]    Script Date: 12/12/2017 15:50:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE TABLE [dbo].[tblCheckNotification](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TypeID] [int] NOT NULL,
	[CheckID] [int] NOT NULL,
	[Active] [bit] NOT NULL,
	[Options] [varchar](max) NOT NULL,
	[Sent] [datetime] NULL,
	[FailCount] [int] NULL,
	[FailMinutes] [int] NULL,
CONSTRAINT [PK_tblCheckNotification] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblCheckNotificationType]    Script Date: 12/12/2017 15:50:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE TABLE [dbo].[tblCheckNotificationType](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NOT NULL,
CONSTRAINT [PK_tblCheckNotificationType] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblCheckNotificationTypeOption]    Script Date: 12/12/2017 15:50:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE TABLE [dbo].[tblCheckNotificationTypeOption](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CheckNotificationTypeID] [int] NOT NULL,
	[OptionTypeID] [int] NOT NULL,
	[Label] [varchar](255) NOT NULL,
	[DefaultValue] [varchar](1) NULL,
	[IsRequired] [bit] NOT NULL,
CONSTRAINT [PK_tblCheckNotificationTypeOption] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblCheckResult]    Script Date: 12/12/2017 15:50:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE TABLE [dbo].[tblCheckResult](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CheckID] [int] NOT NULL,
	[DTS] [datetime] NOT NULL,
	[Status] [int] NOT NULL,
	[TimeMS] [int] NOT NULL,
CONSTRAINT [PK_tblCheckResult] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblCheckSchedule]    Script Date: 12/12/2017 15:50:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE TABLE [dbo].[tblCheckSchedule](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CheckID] [int] NOT NULL,
	[Active] [bit] NOT NULL,
	[Expression] [varchar](255) NOT NULL,
CONSTRAINT [PK_tblCheckSchedule] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblCheckType]    Script Date: 12/12/2017 15:50:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE TABLE [dbo].[tblCheckType](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NOT NULL,
CONSTRAINT [PK_tblCheckType] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblCheckTypeOption]    Script Date: 12/12/2017 15:50:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE TABLE [dbo].[tblCheckTypeOption](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CheckTypeID] [int] NOT NULL,
	[OptionTypeID] [int] NOT NULL,
	[Label] [varchar](255) NOT NULL,
	[DefaultValue] [varchar](255) NULL,
	[IsRequired] [bit] NOT NULL,
CONSTRAINT [PK_tblCheckTypeOption] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblSubCheck]    Script Date: 12/12/2017 15:50:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE TABLE [dbo].[tblSubCheck](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TypeID] [int] NOT NULL,
	[CheckID] [int] NOT NULL,
	[Active] [bit] NOT NULL,
	[Options] [varchar](max) NOT NULL,
CONSTRAINT [PK_tblSubCheck] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblSubCheckType]    Script Date: 12/12/2017 15:50:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE TABLE [dbo].[tblSubCheckType](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CheckTypeID] [int] NOT NULL,
	[Name] [varchar](255) NOT NULL,
CONSTRAINT [PK_tblSubCheckType] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblSubCheckTypeOption]    Script Date: 12/12/2017 15:50:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE TABLE [dbo].[tblSubCheckTypeOption](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SubCheckTypeID] [int] NOT NULL,
	[OptionTypeID] [int] NOT NULL,
	[Label] [varchar](255) NOT NULL,
	[DefaultValue] [varchar](255) NULL,
	[IsRequired] [bit] NOT NULL,
CONSTRAINT [PK_tblSubCheckTypeOption] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[vwChecks]    Script Date: 12/12/2017 15:50:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--------------------------------------------------------------------------------------------------------------------------
-- VIEWS -----------------------------------------------------------------------------------------------------------------
--------------------------------------------------------------------------------------------------------------------------

CREATE VIEW [dbo].[vwChecks]
AS
SELECT c.Name AS CheckName,ct.Name,cd.TypeOptions
FROM tblCheck c
INNER JOIN dbo.tblCheckType ct ON c.TypeID=ct.id
INNER JOIN dbo.tblCheckData cd ON c.DataID=cd.id
GO
/****** Object:  View [dbo].[vwSubChecks]    Script Date: 12/12/2017 15:50:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE VIEW [dbo].[vwSubChecks]
AS
SELECT c.Name AS CheckName,ct.Name,cd.TypeOptions,sc.Options
FROM tblCheck c
INNER JOIN dbo.tblCheckType ct ON c.TypeID=ct.id
INNER JOIN dbo.tblCheckData cd ON c.DataID=cd.id
INNER JOIN dbo.tblSubCheck sc ON sc.CheckID=c.id
GO
/****** Object:  View [dbo].[vwSubCheckTypeOption]    Script Date: 12/12/2017 15:50:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE VIEW [dbo].[vwSubCheckTypeOption] AS
SELECT ct.Name AS CheckType,sct.Name AS SubCheckType, scto.Id AS SubCheckTypeOptionId, scto.Label, scto.DefaultValue, scto.IsRequired
  FROM dbo.tblSubCheckTypeOption scto
  INNER JOIN dbo.tblSubCheckType sct ON sct.id=scto.SubCheckTypeID
  INNER JOIN dbo.tblCheckType ct ON ct.id=sct.CheckTypeID
GO

--------------------------------------------------------------------------------------------------------------------------
-- CONSTRAINTS -----------------------------------------------------------------------------------------------------------
--------------------------------------------------------------------------------------------------------------------------

ALTER TABLE [dbo].[tblCheck]  WITH CHECK ADD CONSTRAINT FK_tblCheck_tblCheckData FOREIGN KEY([DataID])
REFERENCES [dbo].[tblCheckData] ([ID])
GO

ALTER TABLE [dbo].[tblCheck]  WITH CHECK ADD CONSTRAINT FK_tblCheck_tblCheckType FOREIGN KEY([TypeID])
REFERENCES [dbo].[tblCheckType] ([ID])
GO

ALTER TABLE [dbo].[tblCheckNotification]  WITH CHECK ADD CONSTRAINT FK_tblCheckNotification_tblCheck FOREIGN KEY([CheckID])
REFERENCES [dbo].[tblCheck] ([ID])
GO

ALTER TABLE [dbo].[tblCheckNotification]  WITH CHECK ADD CONSTRAINT FK_tblCheckNotification_tblCheckNotificationType FOREIGN KEY([TypeID])
REFERENCES [dbo].[tblCheckNotificationType] ([ID])
GO

ALTER TABLE [dbo].[tblCheckNotificationTypeOption]  WITH CHECK ADD CONSTRAINT FK_tblCheckNotificationTypeOption_tblCheckNotificationType FOREIGN KEY([CheckNotificationTypeID])
REFERENCES [dbo].[tblCheckNotificationType] ([ID])
GO

ALTER TABLE [dbo].[tblCheckResult]  WITH CHECK ADD CONSTRAINT FK_tblCheckResult_tblCheck FOREIGN KEY([CheckID])
REFERENCES [dbo].[tblCheck] ([ID])
GO

ALTER TABLE [dbo].[tblCheckTypeOption]  WITH CHECK ADD CONSTRAINT FK_tblCheckTypeOption_tblCheckType FOREIGN KEY([CheckTypeID])
REFERENCES [dbo].[tblCheckType] ([ID])
GO

ALTER TABLE [dbo].[tblSubCheck]  WITH CHECK ADD CONSTRAINT FK_tblSubCheck_tblCheck FOREIGN KEY([CheckID])
REFERENCES [dbo].[tblCheck] ([ID])
GO

ALTER TABLE [dbo].[tblSubCheck]  WITH CHECK ADD CONSTRAINT FK_tblSubCheck_tblSubCheckType FOREIGN KEY([TypeID])
REFERENCES [dbo].[tblSubCheckType] ([ID])
GO

ALTER TABLE [dbo].[tblSubCheckType]  WITH CHECK ADD CONSTRAINT FK_tblSubCheckType_tblCheckType FOREIGN KEY([CheckTypeID])
REFERENCES [dbo].[tblCheckType] ([ID])
GO

ALTER TABLE [dbo].[tblSubCheckTypeOption]  WITH CHECK ADD CONSTRAINT FK_tblSubCheckTypeOption_tblSubCheckType FOREIGN KEY([SubCheckTypeID])
REFERENCES [dbo].[tblSubCheckType] ([ID])
GO

--------------------------------------------------------------------------------------------------------------------------
-- REFERENCE DATA --------------------------------------------------------------------------------------------------------
--------------------------------------------------------------------------------------------------------------------------

SET IDENTITY_INSERT [tblCheckType] ON;
INSERT INTO [tblCheckType] (Id, Name)
VALUES
( 1, 'Web Request' ), 
( 2, 'Database' ), 
( 3, 'Ping' )
SET IDENTITY_INSERT [tblCheckType] OFF;
GO 

SET IDENTITY_INSERT [tblSubCheckType] ON;
INSERT INTO [tblSubCheckType] ([ID],[CheckTypeID],[Name])
VALUES
( 1, 1, 'Response Contains' ), 
( 2, 1, 'Field Contains' ), 
( 3, 2, 'Field Equal To' ),
( 4, 2, 'Field Not Equal To')
SET IDENTITY_INSERT [tblSubCheckType] OFF;
GO 

SET IDENTITY_INSERT [tblSubCheckTypeOption] ON;
INSERT INTO [tblSubCheckTypeOption] ([ID],[SubCheckTypeID],[OptionTypeID],[Label],[DefaultValue],[IsRequired])
VALUES
( 1, 1, 2, 'Text', NULL, 1 ), 

( 2, 2, 2, 'Field Name (JPath expression)', NULL, 1 ), 
( 3, 2, 1, 'Exists', 'true', 1 ), 
( 4, 2, 2, 'Value contains', NULL, 0 ),

( 6, 3, 2, 'Value', NULL, 0 ), 
( 7, 3, 2, 'Field Name (JPath expression)', NULL, 1 ), 
( 8, 3, 1, 'Exists', 'true', 1 ),

( 9, 4, 2, 'Field Name (JPath expression)', NULL, 1	),
( 10, 4, 2, 'Value', NULL, 0 ),
( 11, 4, 1, 'Exists', 'true', 1 )
SET IDENTITY_INSERT [tblSubCheckTypeOption] OFF;
GO 

SET IDENTITY_INSERT [tblCheckTypeOption] ON;
INSERT INTO [tblCheckTypeOption] ([ID],[CheckTypeID],[OptionTypeID],[Label],[DefaultValue],[IsRequired])
VALUES
( 5, 1, 2, 'Request URL', NULL, 1 ), 
( 6, 1, 5, 'Authentication', NULL, 0 ), 
( 7, 2, 6, 'Connection String', NULL, 1 ), 
( 8, 2, 7, 'SQL Query', NULL, 1 ), 
( 9, 1, 3, 'Timeout (MS)', '5000', 1 ), 
( 10, 1, 3, 'Time Warn (MS)', '4000', 0 ), 
( 12, 3, 2, 'Server Name/IP', NULL, 1 ), 
( 13, 3, 3, 'Timeout (MS)', '120', 1 )
SET IDENTITY_INSERT [tblCheckTypeOption] OFF;
GO 

SET IDENTITY_INSERT [tblCheckNotificationType] ON;
INSERT INTO [tblCheckNotificationType] (Id, Name)
VALUES
( 1, 'Slack' )
SET IDENTITY_INSERT [tblCheckNotificationType] OFF;
GO 

SET IDENTITY_INSERT dbo.tblCheckNotificationTypeOption ON
INSERT INTO dbo.tblCheckNotificationTypeOption
(
	ID,
	CheckNotificationTypeID,
	OptionTypeID,
	Label,
	DefaultValue,
	IsRequired
)
VALUES
( 1, 1, 8, 'Channel',	NULL, 1 )
SET IDENTITY_INSERT dbo.tblCheckNotificationTypeOption OFF
