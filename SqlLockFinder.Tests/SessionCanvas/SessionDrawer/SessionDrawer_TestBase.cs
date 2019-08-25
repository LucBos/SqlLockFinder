using System;
using Moq;
using NUnit.Framework;
using SqlLockFinder.ActivityMonitor;
using SqlLockFinder.Infrastructure;
using SqlLockFinder.SessionCanvas;
using SqlLockFinder.SessionDetail;

namespace SqlLockFinder.Tests.SessionCanvas.SessionDrawer
{
    public abstract class SessionDrawer_TestBase
    {
        protected SqlLockFinder.SessionCanvas.SessionDrawer sessionDrawer;
        protected Mock<ISessionCircleFactory> sessionCircleFactory;
        protected Mock<SqlLockFinder.SessionCanvas.SessionCircleList> sessionCircleList;
        protected Mock<ICanvasWrapper> canvasWrapper;
        protected Mock<ISessionDetail> sessionDetail;
        protected Mock<ILineFactory> lineFactory;

        [SetUp]
        public void BaseSetup()
        {
            sessionCircleFactory = new Mock<ISessionCircleFactory>();
            sessionCircleList = new Mock<SqlLockFinder.SessionCanvas.SessionCircleList>{CallBase = true};
            canvasWrapper = new Mock<ICanvasWrapper>();
            sessionDetail = new Mock<ISessionDetail>();
            lineFactory = new Mock<ILineFactory>();
            sessionDrawer = new SqlLockFinder.SessionCanvas.SessionDrawer(
                sessionCircleFactory.Object,
                sessionCircleList.Object,
                lineFactory.Object,
                canvasWrapper.Object,
                sessionDetail.Object);
        }

        protected Mock<ISessionCircle> CreateSessionCircle(SessionDto session)
        {
            var sessionCircle = new Mock<ISessionCircle>();
            sessionCircle.Setup(x => x.Session).Returns(session);
            sessionCircle.Setup(x => x.UiElement).Returns(new Object());

            sessionCircleFactory
                .Setup(x => x.Create(session, It.IsAny<ISessionCircleList>()))
                .Returns(() => sessionCircle.Object);

            return sessionCircle;
        }

    }
}