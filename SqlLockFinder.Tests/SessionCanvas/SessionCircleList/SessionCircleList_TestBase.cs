using NUnit.Framework;

namespace SqlLockFinder.Tests.SessionCanvas.SessionCircleList
{
    public class SessionCircleList_TestBase
    {
        protected SqlLockFinder.SessionCanvas.SessionCircleList sessionCircleList;

        [SetUp]
        public void BaseSetup()
        {
            sessionCircleList = new SqlLockFinder.SessionCanvas.SessionCircleList();
        }

    }
}