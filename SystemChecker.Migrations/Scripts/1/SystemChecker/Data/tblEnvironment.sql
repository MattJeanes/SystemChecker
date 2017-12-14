IF NOT EXISTS(SELECT 1 FROM tblEnvironment) BEGIN

	INSERT INTO dbo.tblEnvironment
	(
		Name
	)
	VALUES ('Production' -- Name - varchar(255)
		   )

END