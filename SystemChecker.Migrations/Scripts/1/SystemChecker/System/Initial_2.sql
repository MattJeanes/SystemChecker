-- Missed a table in the first one!

IF NOT EXISTS (SELECT 1 FROM dbo.tblCheckNotificationTypeOption WHERE ID=1) BEGIN
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
	(   1,   -- ID - int
	    1,   -- CheckNotificationTypeID - int
		8,   -- OptionTypeID - int
		'Channel',  -- Label - varchar(255)
		NULL,  -- DefaultValue - varchar(1)
		1 -- IsRequired - bit
	)
	SET IDENTITY_INSERT dbo.tblCheckNotificationTypeOption OFF
END