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
            csb.ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=FakeVotingPortalDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;";
            var target = new DatabaseFactory(f, csb, null);
            var jobs = new Job[]
            {
                new Job("TestScripts\\01.CreateMyTable.sql")
            };
            var logger = new ConsoleLogger();
            var sut = new Deployer(jobs, target, logger, null);

            var result = sut.Deploy();

            var expected = new DeployResult(0, 0);
            Assert.Equal(expected, result);
        }
    }

}
