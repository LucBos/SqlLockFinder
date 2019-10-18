using System.Windows.Media;
using SqlLockFinder.ActivityMonitor;
using SqlLockFinder.SessionCanvas;

namespace SqlLockFinder.SessionDetail
{
    public interface ISessionTooltip
    {
        void HideSummary(SessionDto session);
        void ShowSummary(SessionDto session);
    }

    public class SessionTooltip : ISessionTooltip
    {
        private readonly ICanvasWrapper canvasWrapper;
        private SessionOverview sessionOverview;
        private SessionDetail sessionDetail;

        public SessionTooltip(ICanvasWrapper canvasWrapper)
        {
            this.canvasWrapper = canvasWrapper;
        }

        public void HideSummary(SessionDto session)
        {
            if (sessionOverview != null && sessionOverview.Session.SPID == session.SPID)
            {
                canvasWrapper.Remove(sessionOverview);
            }
        }

        public void ShowSummary(SessionDto session)
        {
            if (sessionOverview != null && sessionOverview.Session != session)
            {
                canvasWrapper.Remove(sessionOverview);
            }

            sessionOverview = new SessionOverview(session)
            {
                Background = new SolidColorBrush(Colors.White),
                IsHitTestVisible = false
            };
            canvasWrapper.Add(sessionOverview, 10);
            canvasWrapper.TrackMouse(sessionOverview);
        }
    }
}