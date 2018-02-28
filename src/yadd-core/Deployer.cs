using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Linq;

namespace yadd.core
{

    public class Deployer
    {
        private readonly IEnumerable<Job> jobs;
        private readonly Logger logger;
        private readonly DatabaseFactory historyFactory;
        private readonly DatabaseFactory targetFactory;

        public Deployer(IEnumerable<Job> jobs, DatabaseFactory targetFactory, Logger logger, DatabaseFactory historyFactory)
        {
            this.jobs = jobs;
            this.logger = logger;
            this.historyFactory = historyFactory;
            this.targetFactory = targetFactory;
        }

        public DeployResult Deploy(string outputFile)
        {
            using (var connection = targetFactory.Factory.CreateConnection())
            {
                logger.ConnectingToTargetDatabase();
                connection.ConnectionString = targetFactory.Csb.ConnectionString;
                connection.Open();

                logger.ExportingTargetDatabaseSchema();
                string HistoryTableName = "YaddHistory";
                string exportedSchemaPath = targetFactory.Exporter.ExportSchema(targetFactory.Csb, HistoryTableName);
                var history = new HistoryTable(exportedSchemaPath, HistoryTableName);
                logger.OutputTo(outputFile);
                var executor = new JobExecutor(connection, history, outputFile);

                logger.PreparingTargetDatabase();
                executor.Setup();

                foreach (var job in jobs)
                {
                    using (var transaction = connection.BeginTransaction())
                    {
                        logger.ApplyingScript(job);
                        executor.StartJob(job);
                        foreach (var jobStep in job.GetSteps())
                        {
                            executor.ExecuteStep(jobStep, transaction);
                        }
                        executor.EndJob(job);
                        logger.ScriptApplied(job);

                        transaction.Commit();
                    }
                }

                logger.CleaningUp();
                executor.Teardown();
            }

            logger.AllDone();
            return new DeployResult(0, 0);
        }

        public object Deploy(object outputFile)
        {
            throw new NotImplementedException();
        }
    }
}
