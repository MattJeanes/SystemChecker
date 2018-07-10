SET IDENTITY_INSERT dbo.tblCheckTypeOption ON

INSERT INTO dbo.tblCheckTypeOption
(
	ID,
    CheckTypeID,
    OptionTypeID,
    Label,
    DefaultValue,
    IsRequired,
    Multiple
)
VALUES
(   14,    -- ID - int
    1,    -- CheckTypeID - int
    12,    -- OptionTypeID - int
    'HTTP Method',   -- Label - varchar(255)
    'GET',   -- DefaultValue - varchar(255)
    1, -- IsRequired - bit
    0  -- Multiple - bit
    )

SET IDENTITY_INSERT dbo.tblCheckTypeOption OFF
