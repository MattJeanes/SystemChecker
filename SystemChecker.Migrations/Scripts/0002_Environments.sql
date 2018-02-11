CREATE TABLE [dbo].[tblEnvironment]
(
	[ID] [int] NOT NULL IDENTITY(1, 1) PRIMARY KEY,
	[Name] [varchar] (255) NOT NULL
) ON [PRIMARY]

INSERT INTO dbo.tblEnvironment (Name)
VALUES
('Production')

ALTER TABLE dbo.tblCheck ADD EnvironmentID INT NOT NULL CONSTRAINT Default_tblCheck_TEMP DEFAULT 1
ALTER TABLE dbo.tblCheck DROP CONSTRAINT Default_tblCheck_TEMP

ALTER TABLE dbo.tblCheckConnString ADD EnvironmentID INT NOT NULL CONSTRAINT Default_tblCheckConnString_TEMP DEFAULT 1
ALTER TABLE dbo.tblCheckConnString DROP CONSTRAINT Default_tblCheckConnString_TEMP

ALTER TABLE dbo.tblCheck ADD CONSTRAINT FK_tblCheck_tblEnvironment FOREIGN KEY (EnvironmentID) REFERENCES dbo.tblEnvironment
ALTER TABLE dbo.tblCheckConnString ADD CONSTRAINT FK_tblCheckConnString_tblEnvironment FOREIGN KEY (EnvironmentID) REFERENCES dbo.tblEnvironment