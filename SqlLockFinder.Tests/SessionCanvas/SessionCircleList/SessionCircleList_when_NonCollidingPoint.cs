using System;
using System.Collections.Generic;
using System.Windows.Shapes;
using FluentAssertions;
using NUnit.Framework;
using SqlLockFinder.ActivityMonitor;
using SqlLockFinder.SessionCanvas;
using SqlLockFinder.Tests.Util;

namespace SqlLockFinder.Tests.SessionCanvas.SessionCircleList
{
    public class SessionCircleList_when_NonCollidingPoint: SessionCircleList_TestBase
    {
        [Test]
        public void It_should_find_a_place_for_each_circle_without_colliding()
        {
            sessionCircleList.MaxX = 900;
            sessionCircleList.MaxY = 900;

            var circles = new List<CircleMock>();
            for (int i = 0; i < 100; i++)
            {
                var circle = new CircleMock {Size = Generator.GetRandomNumber(20)};
                var point = sessionCircleList.NonCollidingPoint(circle.Size);
                circle.X = point.X;
                circle.Y = point.Y;

                circles.Add(circle);
            }

            foreach (var circle in sessionCircleList)
            {
                sessionCircleList.Collides(circle).Should().BeFalse();
            }
        }

        public class CircleMock: ISessionCircle
        {
            public Ellipse Ellipse { get; }
            public SessionDto Session { get; set; }
            public object UiElement { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int SpeedX { get; set; }
            public int SpeedY { get; set; }
            public DateTime LastSpeedUpdate { get; set; }
            public int Size { get; set; }
            public bool Selected { get; set; }
            public void OnMouseDown(Action<ISessionCircle> action)
            {
                throw new NotImplementedException();
            }

            public void Move()
            {
                throw new NotImplementedException();
            }

            public void Revert()
            {
                throw new NotImplementedException();
            }

            public void Enable()
            {
                throw new NotImplementedException();
            }

            public void Disable()
            {
                throw new NotImplementedException();
            }
        }
    }
}