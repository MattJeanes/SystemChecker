IF NOT EXISTS (SELECT 1 FROM dbo.tblCheckNotificationTypeOption WHERE ID=2) BEGIN
	SET IDENTITY_INSERT dbo.tblCheckNotificationTypeOption ON
	INSERT INTO dbo.tblCheckNotificationTypeOption
	(
		ID,
	    CheckNotificationTypeID,
	    OptionTypeID,
	    Label,
	    DefaultValue,
	    IsRequired,
        Multiple
	)
	VALUES
	(   2,   -- ID - int
	    2,   -- CheckNotificationTypeID - int
	    10,   -- OptionTypeID - int
	    'Email Addresses',  -- Label - varchar(255)
	    NULL,  -- DefaultValue - varchar(1)
	    1, -- IsRequired - bit
		1 -- Multiple - bit
	)
	SET IDENTITY_INSERT dbo.tblCheckNotificationTypeOption OFF
END

IF NOT EXISTS (SELECT 1 FROM dbo.tblCheckNotificationTypeOption WHERE ID=3) BEGIN
	SET IDENTITY_INSERT dbo.tblCheckNotificationTypeOption ON
	INSERT INTO dbo.tblCheckNotificationTypeOption
	(
		ID,
	    CheckNotificationTypeID,
	    OptionTypeID,
	    Label,
	    DefaultValue,
	    IsRequired,
        Multiple
	)
	VALUES
	(   3,   -- ID - int
	    3,   -- CheckNotificationTypeID - int
	    11,   -- OptionTypeID - int
	    'Phone Numbers',  -- Label - varchar(255)
	    NULL,  -- DefaultValue - varchar(1)
	    1, -- IsRequired - bit
		1 -- Multiple - bit
	)
	SET IDENTITY_INSERT dbo.tblCheckNotificationTypeOption OFF
END