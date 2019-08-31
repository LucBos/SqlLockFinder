using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SqlLockFinder.ActivityMonitor;

namespace SqlLockFinder.Tests.SessionCanvas.SessionDrawer
{
    public class SessionDrawer_when_draw_update_existing_sessions: SessionDrawer_TestBase
    {
        [Test]
        public void It_should_remove_the_sessionsCircle_from_the_canvas()
        {
            var sessions = new List<SessionDto>
            {
                new SessionDto {SPID = 1},
                new SessionDto {SPID = 2}
            };

            var notToUpdate = CreateSessionCircle(sessions.ElementAt(0));
            var toUpdate = CreateSessionCircle(sessions.ElementAt(1));

            sessionDrawer.Draw(sessions);

            var updatedSession = new SessionDto{SPID = 2};
            sessions[1] = updatedSession;

            sessionDrawer.Draw(sessions);

            toUpdate.VerifySet(x => x.Session = updatedSession);
        }
    }
}