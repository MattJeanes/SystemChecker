CREATE VIEW vwLastResultStatus
AS
SELECT c.ID AS CheckID, rs.ID AS StatusID, rs.TypeID AS TypeID FROM dbo.tblCheck c
OUTER APPLY (SELECT TOP(1) rsi.* FROM dbo.tblCheckResult cri INNER JOIN dbo.tblResultStatus rsi ON rsi.ID = cri.StatusID WHERE cri.CheckID=c.ID ORDER BY cri.ID DESC) rs
GO

CREATE NONCLUSTERED INDEX IDX_tblCheckResult_CheckID_ID_StatusID
ON [dbo].[tblCheckResult] ([CheckID])
INCLUDE ([ID],[StatusID])
GO