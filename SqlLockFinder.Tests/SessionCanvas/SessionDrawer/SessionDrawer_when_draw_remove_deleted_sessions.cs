using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using SqlLockFinder.ActivityMonitor;

namespace SqlLockFinder.Tests.SessionCanvas.SessionDrawer
{
    public class SessionDrawer_when_draw_remove_deleted_sessions : SessionDrawer_TestBase
    {
        [Test]
        public void It_should_remove_the_sessionsCircle_from_the_canvas()
        {
            var sessions = new List<SessionDto>
            {
                new SessionDto {SPID = 1},
                new SessionDto {SPID = 2}
            };

            var keepCircle = CreateSessionCircle(sessions.ElementAt(0));
            var removedCircle = CreateSessionCircle(sessions.ElementAt(1));

            sessionDrawer.Draw(sessions);

            sessions.Remove(sessions.ElementAt(1));

            sessionDrawer.Draw(sessions);

            canvasWrapper.Verify(x => x.Remove(removedCircle.Object.UiElement));
            canvasWrapper.Verify(x => x.Remove(keepCircle.Object.UiElement), Times.Never);
        }
    }
}