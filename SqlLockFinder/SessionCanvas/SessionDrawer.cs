using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Shapes;
using SqlLockFinder.ActivityMonitor;
using SqlLockFinder.SessionDetail;

namespace SqlLockFinder.SessionCanvas
{
    public interface ISessionDrawer
    {
        void Draw(List<SessionDto> sessions);
        void Move();
        void Fault();
    }
    public class SessionDrawer : ISessionDrawer
    {
        public static Color FaultColor = Color.FromRgb(250, 166, 166);

        private readonly ISessionCircleFactory sessionCircleFactory;
        private ICanvasWrapper canvas;
        private readonly ISessionDetail sessionDetail;
        private ISessionCircleList sessionCircles;

        public SessionDrawer(ISessionCircleFactory sessionCircleFactory, ISessionCircleList sessionCircles,
            ICanvasWrapper canvas, ISessionDetail sessionDetail)
        {
            this.sessionCircleFactory = sessionCircleFactory;
            this.sessionCircles = sessionCircles;
            this.canvas = canvas;
            this.sessionDetail = sessionDetail;
        }

        public SessionDrawer(ICanvasWrapper canvas, ISessionDetail sessionDetail) : this(new SessionCircleFactory(new SessionTooltip(canvas)), new SessionCircleList(), canvas, sessionDetail)
        { }

        public void Draw(List<SessionDto> sessions)
        {
            canvas.SetColor(Colors.LightGray);
            sessionCircles.MaxX = canvas.ActualWidth;
            sessionCircles.MaxY = canvas.ActualHeight;

            CreateOrUpdate(sessions);
            DestroyOld(sessions);
        }

        public void Move()
        {
            sessionCircles.MaxX = canvas.ActualWidth;
            sessionCircles.MaxY = canvas.ActualHeight;

            foreach (var sessionCircle in sessionCircles)
            {
                if (sessionCircles.Collides(sessionCircle))
                {
                    sessionCircle.Revert();
                }

                sessionCircle.Move();

                canvas.SetPosition(sessionCircle.UiElement, sessionCircle.X, sessionCircle.Y);
            }

            DrawLocks();
        }

        public void Fault()
        {
            canvas.SetColor(FaultColor);
        }

        private void DestroyOld(List<SessionDto> sessions)
        {
            foreach (var sessionCircle in sessionCircles.ToList())
            {
                if (sessions.All(x => sessionCircle.Session.SPID != x.SPID))
                {
                    sessionCircles.Remove(sessionCircle);
                    canvas.Remove(sessionCircle.UiElement);
                }
            }
        }

        private void CreateOrUpdate(List<SessionDto> sessions)
        {
            foreach (var sessionDto in sessions)
            {
                var sessionCircle = sessionCircles.FirstOrDefault(x => x.Session.SPID == sessionDto.SPID);
                if (sessionCircle == null)
                {
                    sessionCircle = sessionCircleFactory.Create(sessionDto, sessionCircles);
                    sessionCircle.MouseDown += SelectSessionCircle;
                    sessionCircles.Add(sessionCircle);
                    canvas.Add(sessionCircle.UiElement, sessionDto.BlockedBy.HasValue ? 3 : 1);
                }

                sessionCircle.Session = sessionDto;
            }
        }

        private void SelectSessionCircle(SessionCircle sessionCircle)
        {
            sessionCircles.DeselectAll();
            sessionCircle.Selected = true;
            sessionDetail.LockedWith = sessionCircles
                .Where(x => x.Session.BlockedBy == sessionCircle.Session.SPID || x.Session.SPID == sessionCircle.Session.BlockedBy)
                .Select(x => x.Session);
            sessionDetail.SessionCircle = sessionCircle;
        }

        private void DrawLocks()
        {
            canvas.RemoveAll<Line>();

            foreach (var sessionCircle in sessionCircles.Where(x => x.Session.BlockedBy.HasValue))
            {
                var blocking = sessionCircles.FirstOrDefault(x => x.Session.SPID == sessionCircle.Session.BlockedBy);
                if (blocking != null)
                {
                    var line = new Line
                    {
                        X1 = sessionCircle.X + sessionCircle.Size / 2,
                        Y1 = sessionCircle.Y + sessionCircle.Size / 2,
                        X2 = blocking.X + sessionCircle.Size / 2,
                        Y2 = blocking.Y + sessionCircle.Size / 2,
                        Stroke = new SolidColorBrush(Colors.Red),
                    };
                    canvas.Add(line, 2);
                }
            }
        }
    }

}