-- Adding datareader/datawriter roles

DECLARE @user VARCHAR(255) = '***REMOVED***\***REMOVED***'

EXEC sys.sp_addrolemember @rolename = 'db_datareader',  -- sysname
                          @membername = @user -- sysname

EXEC sys.sp_addrolemember @rolename = 'db_datawriter',  -- sysname
                          @membername = @user -- sysname