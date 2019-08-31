using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using SqlLockFinder.ActivityMonitor;

namespace SqlLockFinder.Tests.SessionCanvas.SessionDrawer
{
    public class SessionDrawer_when_draw_new_sessions : SessionDrawer_TestBase
    {
        private List<SessionDto> sessions;

        [SetUp]
        public void Setup()
        {
            sessions = new List<SessionDto>
            {
                new SessionDto {SPID = 1}
            };
            CreateSessionCircle(sessions.First());

            sessionDrawer.Draw(sessions);
        }

        [Test]
        public void It_should_add_the_new_sessions_as_sessionCircles_to_the_canvas()
        {
            var newSession = new SessionDto {SPID = 2};
            var newCircle = CreateSessionCircle(newSession);
            sessions.Add(newSession);

            sessionDrawer.Draw(sessions);

            canvasWrapper.Verify(x => x.Add(newCircle.Object.UiElement, It.IsAny<int>()));
        }

        [Test]
        public void Blocked_sessions_should_have_a_higher_z_index_than_unblocked_sessions()
        {
            var newSession1 = new SessionDto { SPID = 2, BlockedBy = 5};
            var newCircle1 = CreateSessionCircle(newSession1);
            sessions.Add(newSession1);

            var newSession2 = new SessionDto { SPID = 3 };
            var newCircle2 = CreateSessionCircle(newSession2);
            sessions.Add(newSession2);

            sessionDrawer.Draw(sessions);

            canvasWrapper.Verify(x => x.Add(newCircle1.Object.UiElement, 3));
            canvasWrapper.Verify(x => x.Add(newCircle2.Object.UiElement, 1));
        }
    }
}