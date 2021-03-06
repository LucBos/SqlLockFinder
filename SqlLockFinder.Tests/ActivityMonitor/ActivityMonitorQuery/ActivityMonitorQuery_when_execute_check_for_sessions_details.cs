﻿using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace SqlLockFinder.Tests.ActivityMonitor.ActivityMonitorQuery
{
    class ActivityMonitorQuery_when_execute_check_for_sessions_details: ActivityMonitorTestBase
    {
        [Test]
        public async Task It_should_return_the_resource_information_of_each_session()
        {
            PerformIntensiveSqlTask();

            var queryResult = await new SqlLockFinder.ActivityMonitor.ActivityMonitorQuery(new TestConnectionContainer())
                .Execute();

            queryResult.Result.Should().Contain(x =>
                x.DatabaseName == "Northwind"
                && x.TotalSessionCPUms > 0
                && x.HostName.Trim() == Environment.MachineName
                && x.OpenTransactions >= 1);
        }

    }
}