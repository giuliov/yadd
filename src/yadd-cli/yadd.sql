
--[*]-- Script created on 2018-02-27T19:17:31.3413157+00:00

/* make sure YADD History table exists; DBA can create a compatible one */
IF (NOT EXISTS
        (SELECT *
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_NAME='YaddHistory'
            AND   TABLE_TYPE = 'BASE TABLE'))
BEGIN
    PRINT N'Creating table YaddHistory.';  
    CREATE TABLE YaddHistory (
        Id              INT             NOT NULL IDENTITY(1,1) PRIMARY KEY,
        RecordType      CHAR(1)         NOT NULL,
        Title           VARCHAR(140)    NOT NULL,
        BaseVersion     BINARY(20)      NULL,
        ScriptVersion   BINARY(20)      NOT NULL,
        Username        VARCHAR(140)    NOT NULL,
        StartDate       DATETIMEOFFSET  NOT NULL,
        FinishDate      DATETIMEOFFSET  NULL,
        RecordHash      BINARY(20)      NULL,
        Description     NVARCHAR(500)   NOT NULL DEFAULT ''
    )
END
GO

/* Baseline record for this database version */
IF (NOT EXISTS
        (SELECT *
            FROM YaddHistory
            WHERE RecordType = 'B'
            AND   BaseVersion = 0xE825D1D7381263DD48EFAA9B0F8D14FF795D2B91))
BEGIN
    PRINT N'Inserting baseline record.';  
    INSERT INTO YaddHistory
      (RecordType,Title,BaseVersion,ScriptVersion,Username,StartDate,Description)
    VALUES
      ( 'B', 'Initial YADD baseline', 0xE825D1D7381263DD48EFAA9B0F8D14FF795D2B91, 0xE825D1D7381263DD48EFAA9B0F8D14FF795D2B91, 'gvian', SYSDATETIMEOFFSET(), 'YADD baseline automatically created' )
END
GO

--[*]-- Start Job 01.CreateMyTable

/* record attempt to run 01.CreateMyTable script */
IF (NOT EXISTS
        (SELECT *
            FROM YaddHistory
            WHERE RecordType = 'F'
            AND   ScriptVersion = 0xC6AD04F40A6095ED4FF083D6E1DF03679CF117E6))
BEGIN
    PRINT N'01.CreateMyTable has not run, adding record to history table.';  
    INSERT INTO YaddHistory
      (RecordType,Title,BaseVersion,ScriptVersion,Username,StartDate,Description)
    VALUES
      ( 'F', '01.CreateMyTable', 0xE825D1D7381263DD48EFAA9B0F8D14FF795D2B91, 0xC6AD04F40A6095ED4FF083D6E1DF03679CF117E6, 'gvian', SYSDATETIMEOFFSET(), '' )
END
GO

--[*]-- Job 01.CreateMyTable, Step #1 --[*]--
IF (EXISTS
        (SELECT *
            FROM YaddHistory
            WHERE RecordType = 'F'
            AND   ScriptVersion = 0xC6AD04F40A6095ED4FF083D6E1DF03679CF117E6
            AND   FinishDate IS NULL))
BEGIN
    PRINT N'Executing step #1 of 01.CreateMyTable.';  
    -- just a comment
SELECT 'SCRIPT APPLIED';

END
GO

/* record 01.CreateMyTable script has run */
UPDATE YaddHistory
SET FinishDate = SYSDATETIMEOFFSET(),
    RecordHash = 0x73F4DEC360303CA71C723B710D1824E979ECEAB5
WHERE Id = IDENT_CURRENT('YaddHistory')
AND   RecordType = 'F'
AND   ScriptVersion = 0xC6AD04F40A6095ED4FF083D6E1DF03679CF117E6
AND   FinishDate IS NULL

IF @@ROWCOUNT <> 0
    PRINT N'01.CreateMyTable has run, record in history table is closed.';  
GO

GO

--[*]-- End of Job 01.CreateMyTable
