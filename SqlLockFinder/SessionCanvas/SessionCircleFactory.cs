using SqlLockFinder.ActivityMonitor;
using SqlLockFinder.Infrastructure;
using SqlLockFinder.SessionDetail;

namespace SqlLockFinder.SessionCanvas
{
    public interface ISessionCircleFactory
    {
        ISessionCircle Create(SessionDto sessionDto, ISessionCircleList position);
    }

    public class SessionCircleFactory : ISessionCircleFactory
    {
        private const int DefaultSize = 30;
        private readonly ISessionTooltip sessionTooltip;

        public SessionCircleFactory(ISessionTooltip sessionTooltip)
        {
            this.sessionTooltip = sessionTooltip;
        }

        public ISessionCircle Create(SessionDto sessionDto, ISessionCircleList sessionCircles)
        {
            var position = sessionCircles.NonCollidingPoint(DefaultSize);
            var sessionCirlce = new SessionCircle
            {
                Session = sessionDto,
                SpeedX = GlobalRandom.Instance.Next(-1, 2),
                SpeedY = GlobalRandom.Instance.Next(-1, 2),
                X = position.X,
                Y = position.Y,
                Size = DefaultSize,
            };

            sessionCirlce.MouseOver += circle => sessionTooltip.ShowSummary(circle.Session);
            sessionCirlce.MouseLeave += circle => sessionTooltip.HideSummary(circle.Session);

            return sessionCirlce;
        }
    }
}