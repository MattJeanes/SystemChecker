IF NOT EXISTS(SELECT 1 FROM dbo.tblSubCheckType WHERE ID=5) BEGIN
	SET IDENTITY_INSERT dbo.tblSubCheckType ON

	INSERT INTO dbo.tblSubCheckType
	(
		ID,
		CheckTypeID,
		Name
	)
	VALUES
	(   5, -- ID - int
		2, -- CheckTypeID - int
		'Field Greater Than' -- Name - varchar(255)
	)

	INSERT INTO dbo.tblSubCheckType
	(
		ID,
		CheckTypeID,
		Name
	)
	VALUES
	(   6, -- ID - int
		2, -- CheckTypeID - int
		'Field Less Than' -- Name - varchar(255)
	)

	SET IDENTITY_INSERT dbo.tblSubCheckType OFF
END