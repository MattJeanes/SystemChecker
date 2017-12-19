IF NOT EXISTS (SELECT 1 FROM dbo.tblContactType WHERE ID=1) BEGIN
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
END

IF NOT EXISTS (SELECT 1 FROM dbo.tblContactType WHERE ID=2) BEGIN
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
END