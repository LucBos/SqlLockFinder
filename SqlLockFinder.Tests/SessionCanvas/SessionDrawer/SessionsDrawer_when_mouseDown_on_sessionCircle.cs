using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using SqlLockFinder.ActivityMonitor;
using SqlLockFinder.SessionCanvas;

namespace SqlLockFinder.Tests.SessionCanvas.SessionDrawer
{
    public class SessionsDrawer_when_mouseDown_on_sessionCircle : SessionDrawer_TestBase
    {
        private List<SessionDto> sessions;
        private Mock<ISessionCircle> circle1, circle2, circle3, circle4;
        private Action<ISessionCircle> mouseDown;

        [SetUp]
        public void Setup()
        {
            sessions = new List<SessionDto>
            {
                new SessionDto {SPID = 1, BlockedBy = 3},
                new SessionDto {SPID = 2, BlockedBy = 1},
                new SessionDto {SPID = 3},
                new SessionDto {SPID = 4}
            };

            circle1 = CreateSessionCircle(sessions.ElementAt(0));
            circle2 = CreateSessionCircle(sessions.ElementAt(1));
            circle3 = CreateSessionCircle(sessions.ElementAt(2));
            circle4 = CreateSessionCircle(sessions.ElementAt(3));

            circle1.Setup(x => x.OnMouseDown(It.IsAny<Action<ISessionCircle>>()))
                .Callback((Action<ISessionCircle> action) => mouseDown = action);
        }

        [Test]
        public void It_should_select_the_sessionCircle()
        {
            sessionDrawer.Draw(sessions);
            sessionDrawer.Move();
            mouseDown.Invoke(circle1.Object);

            circle1.VerifySet(x => x.Selected = true);
        }

        [Test]
        public void It_should_deselect_all_other_sessionCircles()
        {
            sessionDrawer.Draw(sessions);
            sessionDrawer.Move();
            mouseDown.Invoke(circle1.Object);

            circle2.VerifySet(x => x.Selected = false);
            circle3.VerifySet(x => x.Selected = false);
            circle4.VerifySet(x => x.Selected = false);
        }

        [Test]
        public void It_should_update_the_sessionCircle_on_the_sessionDetail_pane()
        {
            sessionDrawer.Draw(sessions);
            sessionDrawer.Move();
            mouseDown.Invoke(circle1.Object);

            sessionDetail.VerifySet(x => x.SessionCircle = circle1.Object);
        }

        [Test]
        public void It_should_update_the_locked_sessions_on_the_sessionDetail_pane()
        {
            sessionDrawer.Draw(sessions);
            sessionDrawer.Move();
            mouseDown.Invoke(circle1.Object);

            sessionDetail.VerifySet(x => x.SessionCircle = circle1.Object);
        }
    }
}