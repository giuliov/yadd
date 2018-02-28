using System;
using System.Data.Common;
using System.IO;

namespace yadd.core
{
    internal class JobExecutor
    {
        private DbConnection connection;
        private HistoryTable history;
        private HistoryRecord record;

        public JobExecutor(DbConnection connection, HistoryTable history)
        {
            this.connection = connection;
            this.history = history;
            OutputFile = "yadd.sql";
            File.WriteAllText(OutputFile, $@"
--[*]-- Script created on {DateTimeOffset.UtcNow:o}
");
        }

        public string OutputFile { get; private set; }

        internal void Setup()
        {
            File.AppendAllText(OutputFile, $@"
/* make sure YADD History table exists; DBA can create a compatible one */
IF (NOT EXISTS
        (SELECT *
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_NAME='{history.TableName}'
            AND   TABLE_TYPE = 'BASE TABLE'))
BEGIN
    PRINT N'Creating table {history.TableName}.';  
    CREATE TABLE {history.TableName} (
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
");
            File.AppendAllText(OutputFile, $@"
/* Baseline record for this database version */
IF (NOT EXISTS
        (SELECT *
            FROM {history.TableName}
            WHERE RecordType = 'B'
            AND   BaseVersion = 0x{history.BaselineVersion}))
BEGIN
    PRINT N'Inserting baseline record.';  
    INSERT INTO {history.TableName}
      (RecordType,Title,BaseVersion,ScriptVersion,Username,StartDate,Description)
    VALUES
      ( 'B', 'Initial YADD baseline', 0x{history.BaselineVersion}, 0x{history.BaselineVersion}, '{history.Username}', SYSDATETIMEOFFSET(), 'YADD baseline automatically created' )
END
GO
");
        }

        internal void Teardown()
        {
            // no-op for now
        }

        internal void StartJob(Job job)
        {
            this.record = history.AddRecord(job);
            File.AppendAllText(OutputFile, $@"
--[*]-- Start Job {job.Name}
");
            File.AppendAllText(OutputFile, $@"
/* record attempt to run {record.Title} script */
IF (NOT EXISTS
        (SELECT *
            FROM {history.TableName}
            WHERE RecordType = '{(char)record.RecordType}'
            AND   Title = '{record.Title}'))
BEGIN
    PRINT N'{record.Title} has not run, adding record to history table.';  
    INSERT INTO {history.TableName}
      (RecordType,Title,BaseVersion,ScriptVersion,Username,StartDate,Description)
    VALUES
      ( '{(char)record.RecordType}', '{record.Title}', 0x{record.BaseVersion}, 0x{record.ScriptVersion}, '{record.Username}', SYSDATETIMEOFFSET(), '{record.Description}' )
END ELSE IF (EXISTS
        (SELECT *
            FROM {history.TableName}
            WHERE RecordType = '{(char)record.RecordType}'
            AND   Title = '{record.Title}'
            AND   ScriptVersion <> 0x{record.ScriptVersion}))
BEGIN
    RAISERROR( N'A different version of [{record.Title}] already run, but script content has changed.', 18, 1 );  
END
GO
");
        }

        internal void ExecuteStep(JobStep jobStep, DbTransaction transaction)
        {
            record.TrackSuccess(jobStep);
            File.AppendAllText(OutputFile, $@"
--[*]-- Job {jobStep.Parent.Name}, Step #{jobStep.Number} --[*]--
IF (EXISTS
        (SELECT *
            FROM {history.TableName}
            WHERE RecordType = '{(char)record.RecordType}'
            AND   ScriptVersion = 0x{record.ScriptVersion}
            AND   FinishDate IS NULL))
BEGIN
    PRINT N'Executing step #{jobStep.Number} of {jobStep.Parent.Name}.';  
    {jobStep.Command}
END
GO
");
        }

        internal void EndJob(Job job)
        {
            record.Close();
            File.AppendAllText(OutputFile, $@"
/* record {record.Title} script has run */
UPDATE {history.TableName}
SET FinishDate = SYSDATETIMEOFFSET(),
    RecordHash = 0x{record.GetHash()}
WHERE Id = IDENT_CURRENT('{history.TableName}')
AND   RecordType = '{(char)record.RecordType}'
AND   ScriptVersion = 0x{record.ScriptVersion}
AND   FinishDate IS NULL

IF @@ROWCOUNT <> 0
    PRINT N'{record.Title} has run, record in history table is closed.';  
GO

GO
");
            File.AppendAllText(OutputFile, $@"
--[*]-- End of Job {job.Name}
");
        }
    }
}