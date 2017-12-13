IF ((SELECT Label FROM dbo.tblSubCheckTypeOption WHERE ID=6) = 'Value equals (single row)') BEGIN
	UPDATE dbo.tblSubCheckTypeOption SET Label='Value' WHERE ID=6
END

IF NOT EXISTS(SELECT 1 FROM dbo.tblSubCheckTypeOption WHERE SubCheckTypeID=4) BEGIN
	SET IDENTITY_INSERT dbo.tblSubCheckTypeOption ON

	INSERT INTO dbo.tblSubCheckTypeOption
	(
		ID,
		SubCheckTypeID,
		OptionTypeID,
		Label,
		DefaultValue,
		IsRequired
	)
	VALUES
	(   9,   -- ID - int
	    4,   -- SubCheckTypeID - int
		2,   -- OptionTypeID - int
		'Field Name (JPath expression)',  -- Label - varchar(255)
		NULL,  -- DefaultValue - varchar(255)
		1 -- IsRequired - bit
	),
	(   10,   -- ID - int
	    4,   -- SubCheckTypeID - int
		2,   -- OptionTypeID - int
		'Value',  -- Label - varchar(255)
		NULL,  -- DefaultValue - varchar(255)
		0 -- IsRequired - bit
	),
	(   11,   -- ID - int
	    4,   -- SubCheckTypeID - int
		1,   -- OptionTypeID - int
		'Exists',  -- Label - varchar(255)
		'true',  -- DefaultValue - varchar(255)
		1 -- IsRequired - bit
	)
	SET IDENTITY_INSERT dbo.tblSubCheckTypeOption OFF

END