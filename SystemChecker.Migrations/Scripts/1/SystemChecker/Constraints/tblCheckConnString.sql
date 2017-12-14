IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_NAME='tblCheckConnString' AND CONSTRAINT_NAME='FK_tblCheckConnString_tblEnvironment') BEGIN
	ALTER TABLE dbo.tblCheckConnString ADD CONSTRAINT FK_tblCheckConnString_tblEnvironment FOREIGN KEY (EnvironmentID) REFERENCES dbo.tblEnvironment
END