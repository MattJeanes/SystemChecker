IF (NOT EXISTS (SELECT 1 FROM dbo.tblCheckNotificationType WHERE ID=2)) BEGIN
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
END


IF (NOT EXISTS (SELECT 1 FROM dbo.tblCheckNotificationType WHERE ID=3)) BEGIN
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
END