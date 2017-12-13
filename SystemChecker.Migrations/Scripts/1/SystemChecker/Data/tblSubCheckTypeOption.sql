IF ((SELECT Label FROM dbo.tblSubCheckTypeOption WHERE ID=7) = 'Column Name (JPath expression)') BEGIN
	UPDATE dbo.tblSubCheckTypeOption SET Label='Field Name (JPath expression)' WHERE ID=7
END

IF ((SELECT OptionTypeID FROM dbo.tblSubCheckTypeOption WHERE ID=6) = 3) BEGIN
	UPDATE dbo.tblSubCheckTypeOption SET OptionTypeID=2 WHERE ID=6
END