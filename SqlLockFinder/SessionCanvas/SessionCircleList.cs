using System;
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
            const int MaxLoop = 100000;
            int x = 0;
            int y = 0;
            var maxX = MaxX - (size * 2);
            var maxY = MaxY - (size * 2);
            if (maxY <= (size * 2) || maxX <= (size * 2))
            {
                return new Point(0,0); // canvas to small to draw
            }
            for (int i = 0; i < MaxLoop; i++)
            {
                x = GlobalRandom.Instance.Next((size * 2), maxX);
                y = GlobalRandom.Instance.Next((size * 2), maxY);
                if (!Collides(x, y, size))
                {
                    return new Point(x, y);
                }
            }
            return new Point(x, y); // if the screen is too small then collisions will occur
        }

        public virtual bool Collides(ISessionCircle sessionCircle)
        {
            return CollidesBoundry(sessionCircle.X, sessionCircle.Y, sessionCircle.Size) 
                   || this.Any(circle => circle != sessionCircle
                                      && CollidesX(sessionCircle.X, sessionCircle.Size, circle)
                                      && CollidesY(sessionCircle.Y, sessionCircle.Size, circle));
        }

        private bool CollidesBoundry(int x, int y, int size)
        {
            return y <= 0
                   || y + size >= MaxY
                   || x <= 0
                   || x + size >= MaxX;
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

        public virtual int MaxX { get; set; }
        public virtual int MaxY { get; set; }

        public void DeselectAll()
        {
            foreach (var sessionCircle in this)
            {
                sessionCircle.Selected = false;
            }
        }
    }
}