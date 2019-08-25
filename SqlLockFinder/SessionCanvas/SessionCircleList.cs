using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SqlLockFinder.Infrastructure;

namespace SqlLockFinder.SessionCanvas
{
    public interface ISessionCircleList : IList<ISessionCircle>
    {
        Point NonCollidingPoint(int size);
        bool Collides(ISessionCircle sessionCircle);
        int MaxX { get; set; }
        int MaxY { get; set; }
        void DeselectAll();
    }

    public class SessionCircleList : List<ISessionCircle>, ISessionCircleList
    {
        public Point NonCollidingPoint(int size)
        {
            int x;
            int y;
            do
            {
                x = GlobalRandom.Instance.Next((size * 2), MaxX - (size * 2));
                y = GlobalRandom.Instance.Next((size * 2), MaxY - (size * 2));
            } while (Collides(x, y, size));

            return new Point(x, y);
        }

        public bool Collides(ISessionCircle sessionCircle)
        {
            return CollidesBoundry(sessionCircle) 
                   || this.Any(circle => circle != sessionCircle
                                      && CollidesX(sessionCircle.X, sessionCircle.Size, circle)
                                      && CollidesY(sessionCircle.Y, sessionCircle.Size, circle));
        }

        private bool CollidesBoundry(ISessionCircle sessionCircle)
        {
            return sessionCircle.Y <= 0
                   || sessionCircle.Y + sessionCircle.Size >= MaxY
                   || sessionCircle.X <= 0
                   || sessionCircle.X + sessionCircle.Size >= MaxX;
        }

        public bool Collides(int x, int y, int size)
        {
            return this.Any(circe => CollidesX(x, size, circe) && CollidesY(y, size, circe));
        }

        private bool CollidesY(int y, int size, ISessionCircle cirlce)
        {
            return (cirlce.Y + cirlce.Size >= y && cirlce.Y <= y + size);
        }

        private bool CollidesX(int x, int size, ISessionCircle cirlce)
        {
            return cirlce.X + cirlce.Size >= x && cirlce.X <= x + size;
        }

        public int MaxX { get; set; }
        public int MaxY { get; set; }

        public void DeselectAll()
        {
            foreach (var sessionCircle in this)
            {
                sessionCircle.Selected = false;
            }
        }
    }
}