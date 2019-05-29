
--[*]-- Script created on 2018-02-28T10:07:31.4477929+00:00

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
        BaseVersion     BINARY(21)      NULL,
        ScriptVersion   BINARY(21)      NOT NULL,
        Username        VARCHAR(140)    NOT NULL,
        StartDate       DATETIMEOFFSET  NOT NULL,
        FinishDate      DATETIMEOFFSET  NULL,
        RecordHash      BINARY(21)      NULL,
        UserSignature   BINARY(21)      NULL,
        Description     NVARCHAR(500)   NOT NULL DEFAULT ''
    )
END
GO

/* Baseline record for this database version */
IF (NOT EXISTS
        (SELECT *
            FROM YaddHistory
            WHERE RecordType = 'B'
            AND   BaseVersion = 0xBAC3B0BF305A629A56236A09B3E533DE436A04E5A1))
BEGIN
    PRINT N'Inserting baseline record.';  
    INSERT INTO YaddHistory
      (RecordType,Title,BaseVersion,ScriptVersion,Username,StartDate,Description)
    VALUES
      ( 'B', 'Initial YADD baseline', 0xBAC3B0BF305A629A56236A09B3E533DE436A04E5A1, 0xBAC3B0BF305A629A56236A09B3E533DE436A04E5A1, 'gvian', SYSDATETIMEOFFSET(), 'YADD baseline automatically created' )
END
GO

--[*]-- Start Job 01.CreateMyTable

/* record attempt to run 01.CreateMyTable script */
IF (NOT EXISTS
        (SELECT *
            FROM YaddHistory
            WHERE RecordType = 'F'
            AND   Title = '01.CreateMyTable'))
BEGIN
    PRINT N'01.CreateMyTable has not run, adding record to history table.';  
    INSERT INTO YaddHistory
      (RecordType,Title,BaseVersion,ScriptVersion,Username,StartDate,Description)
    VALUES
      ( 'F', '01.CreateMyTable', 0xBAC3B0BF305A629A56236A09B3E533DE436A04E5A1, 0xC6AD04F40A6095ED4FF083D6E1DF03679CF117E6A1, 'gvian', SYSDATETIMEOFFSET(), '' )
END ELSE IF (EXISTS
        (SELECT *
            FROM YaddHistory
            WHERE RecordType = 'F'
            AND   Title = '01.CreateMyTable'
            AND   ScriptVersion <> 0xC6AD04F40A6095ED4FF083D6E1DF03679CF117E6A1))
BEGIN
    RAISERROR( N'A different version of [01.CreateMyTable] already run, but script content has changed.', 18, 1 );  
END
GO

--[*]-- Job 01.CreateMyTable, Step #1 --[*]--
IF (EXISTS
        (SELECT *
            FROM YaddHistory
            WHERE RecordType = 'F'
            AND   ScriptVersion = 0xC6AD04F40A6095ED4FF083D6E1DF03679CF117E6A1
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
    RecordHash = 0xEA2DC7ED81B7AE29C7959EEA491B99E7C631B7A6A1
WHERE Id = IDENT_CURRENT('YaddHistory')
AND   RecordType = 'F'
AND   ScriptVersion = 0xC6AD04F40A6095ED4FF083D6E1DF03679CF117E6A1
AND   FinishDate IS NULL

IF @@ROWCOUNT <> 0
    PRINT N'01.CreateMyTable has run, record in history table is closed.';  
GO

GO

--[*]-- End of Job 01.CreateMyTable
