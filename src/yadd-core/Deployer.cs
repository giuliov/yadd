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
        private readonly DbProviderFactory targetFactory;
        private readonly string targetConnectionString;

        public Deployer(IEnumerable<Job> jobs, DatabaseFactory targetFactory, Logger logger, DatabaseFactory historyFactory)
        {
            this.jobs = jobs;
            this.logger = logger;
            this.historyFactory = historyFactory;
            this.targetFactory = targetFactory.Factory;
            this.targetConnectionString = targetFactory.Csb.ConnectionString;
        }

        public DeployResult Deploy()
        {
            using (var connection = targetFactory.CreateConnection())
            {
                connection.ConnectionString = targetConnectionString;
                connection.Open();

                var history = new HistoryTable(connection);

                foreach (var job in jobs)
                {
                    using (var transaction = connection.BeginTransaction())
                    {
                        var record = history.AddRecord(job.Name);
                        foreach (var jobStep in job.Steps)
                        {
                            jobStep.Execute(transaction);
                            record.TrackSuccess(jobStep);
                        }

                        record.Close();
                        transaction.Commit();
                    }
                }
            }


            return new DeployResult(0, 0);
        }
    }
}
