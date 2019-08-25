using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Shapes;
using Moq;
using NUnit.Framework;
using SqlLockFinder.ActivityMonitor;
using SqlLockFinder.SessionCanvas;

namespace SqlLockFinder.Tests.SessionCanvas.SessionDrawer
{
    public class SessionDrawer_when_move_draw_locks: SessionDrawer_TestBase
    {
        private List<SessionDto> sessions;
        private Mock<ISessionCircle> circle1, circle2, circle3, circle4, circle5;

        [SetUp]
        public void Setup()
        {
            sessions = new List<SessionDto>
            {
                new SessionDto {SPID = 1},
                new SessionDto {SPID = 2, BlockedBy = 1},
                new SessionDto {SPID = 3},
                new SessionDto {SPID = 4, BlockedBy = 3},
                new SessionDto {SPID = 5},
            };

            circle1 = CreateSessionCircle(sessions.ElementAt(0));
            circle2 = CreateSessionCircle(sessions.ElementAt(1));
            circle3 = CreateSessionCircle(sessions.ElementAt(2));
            circle4 = CreateSessionCircle(sessions.ElementAt(3));
            circle5 = CreateSessionCircle(sessions.ElementAt(4));
        }

        [Test]
        public void It_should_remove_all_previous_lines()
        {
            sessionDrawer.Draw(sessions);
            sessionDrawer.Move();

            canvasWrapper.Verify(x => x.RemoveAll<Line>());
        }

        [Test]
        public void It_should_create_a_line_to_and_from_each_blocked_and_blocking_sessionsCircle()
        {
            circle1.Setup(x => x.Size).Returns(10);
            circle2.Setup(x => x.Size).Returns(12);
            circle3.Setup(x => x.Size).Returns(14);
            circle4.Setup(x => x.Size).Returns(16);
            circle5.Setup(x => x.Size).Returns(18);

            circle1.Setup(x => x.X).Returns(100);
            circle2.Setup(x => x.X).Returns(200);
            circle3.Setup(x => x.X).Returns(300);
            circle4.Setup(x => x.X).Returns(400);
            circle5.Setup(x => x.X).Returns(500);

            circle1.Setup(x => x.Y).Returns(1000);
            circle2.Setup(x => x.Y).Returns(2000);
            circle3.Setup(x => x.Y).Returns(3000);
            circle4.Setup(x => x.Y).Returns(4000);
            circle5.Setup(x => x.Y).Returns(5000);

            var expectedLine1 = new object();
            var expectedLine2 = new object();

            lineFactory.Setup(x => x.Create(206, 2006, 105, 1005, Colors.Red))
                .Returns(expectedLine1);

            lineFactory.Setup(x => x.Create(408, 4008, 307, 3007, Colors.Red))
                .Returns(expectedLine2);

            sessionDrawer.Draw(sessions);
            sessionDrawer.Move();

            canvasWrapper.Verify(x=> x.Add(expectedLine1, 2));
            canvasWrapper.Verify(x=> x.Add(expectedLine2, 2));
            canvasWrapper.Verify(x=> x.Add(It.IsAny<object>(), 2), Times.Exactly(2));
        }
    }
}