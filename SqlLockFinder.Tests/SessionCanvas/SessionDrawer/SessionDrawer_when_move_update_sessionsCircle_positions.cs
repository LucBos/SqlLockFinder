using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using SqlLockFinder.ActivityMonitor;
using SqlLockFinder.SessionCanvas;
using SqlLockFinder.Tests.Util;

namespace SqlLockFinder.Tests.SessionCanvas.SessionDrawer
{
    public class SessionDrawer_when_move_update_sessionsCircle_positions : SessionDrawer_TestBase
    {
        private List<SessionDto> sessions;
        private Mock<ISessionCircle> circle1, circle2, circle3;

        [SetUp]
        public void Setup()
        {
            sessions = new List<SessionDto>
            {
                new SessionDto {SPID = 1},
                new SessionDto {SPID = 2},
                new SessionDto {SPID = 3}
            };

            circle1 = CreateSessionCircle(sessions.ElementAt(0));
            circle2 = CreateSessionCircle(sessions.ElementAt(1));
            circle3 = CreateSessionCircle(sessions.ElementAt(2));
        }

        [Test]
        public void It_should_update_the_canvas_height_and_width()
        {
            var actualHeight = Generator.GetRandomNumber(99999);
            var actualWidth = Generator.GetRandomNumber(99999);
            canvasWrapper.Setup(x => x.ActualHeight).Returns(actualHeight);
            canvasWrapper.Setup(x => x.ActualWidth).Returns(actualWidth);

            sessionDrawer.Draw(sessions);
            sessionDrawer.Move();

            sessionCircleList.VerifySet(x => x.MaxX = actualWidth);
            sessionCircleList.VerifySet(x => x.MaxY = actualHeight);
        }

        [Test]
        public void It_should_revert_the_vector_of_colliding_sessionCircles()
        {
            sessionCircleList.Setup(x => x.Collides(circle1.Object)).Returns(() => false);
            sessionCircleList.Setup(x => x.Collides(circle2.Object)).Returns(() => true);
            sessionCircleList.Setup(x => x.Collides(circle3.Object)).Returns(() => false);

            sessionDrawer.Draw(sessions);
            sessionDrawer.Move();

            circle1.Verify(x => x.Revert(), Times.Never);
            circle2.Verify(x => x.Revert(), Times.Once);
            circle3.Verify(x => x.Revert(), Times.Never);
        }

        [Test]
        public void It_should_update_the_position_of_each_sessionCircle()
        {
            sessionDrawer.Draw(sessions);
            sessionDrawer.Move();

            circle1.Verify(x => x.Move(), Times.Once);
            circle2.Verify(x => x.Move(), Times.Once);
            circle3.Verify(x => x.Move(), Times.Once);
        }
    }
}