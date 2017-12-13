IF ((SELECT Name FROM dbo.tblSubCheckType WHERE ID=3) = 'JSON Property') BEGIN
	UPDATE dbo.tblSubCheckType SET Name='Field Equal To' WHERE ID=3
END

IF NOT EXISTS(SELECT 1 FROM dbo.tblSubCheckType WHERE ID=4) BEGIN
	SET IDENTITY_INSERT dbo.tblSubCheckType ON

	INSERT INTO dbo.tblSubCheckType
	(
		ID,
		CheckTypeID,
		Name
	)
	VALUES
	(   4, -- ID - int
		2, -- CheckTypeID - int
		'Field Not Equal To' -- Name - varchar(255)
	)

	SET IDENTITY_INSERT dbo.tblSubCheckType OFF
END