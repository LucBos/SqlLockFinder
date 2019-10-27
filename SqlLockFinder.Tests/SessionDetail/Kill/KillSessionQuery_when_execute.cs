using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FluentAssertions;
using NUnit.Framework;
using SqlLockFinder.SessionDetail.Kill;

namespace SqlLockFinder.Tests.SessionDetail.Kill
{
    public class KillSessionQuery_when_execute: DoubleConnection_TestBase
    {
        [Test]
        public async Task It_should_kill_the_session()
        {
            var spid1 = connection1.Query<int>("SELECT @@SPID", transaction:transaction1).First();

            await new KillSessionQuery(new TestConnectionContainer()).Execute(spid1);

            var spids = connection2.Query<int>("SELECT spid FROM sys.sysprocesses", transaction: transaction2).ToList();
            spids.Should().NotContain(spid1);
        }
    }
}