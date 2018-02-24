using System;
using System.Collections.Generic;
using System.Data.Common;
using Xunit;
using yadd;
using yadd.core;
using System.Data.SqlClient;

namespace yadd_core.UnitTest
{
    public class DeployerTests
    {
        [Fact]
        public void Test1()
        {
            DbProviderFactory f = SqlClientFactory.Instance;
            var csb = f.CreateConnectionStringBuilder();
            var jobs = new Job[]
            {
                new Job("")
            };
            var logger = new ConsoleLogger();
            var sut = new Deployer(jobs, new DatabaseFactory(f, csb), logger, null);

            var result = sut.Deploy();

            var expected = new DeployResult(0, 0);
            Assert.Equal(expected, result);
        }
    }

}
