using FluentAssertions;
using Moq;
using NUnit.Framework;
using SqlLockFinder.SessionCanvas;

namespace SqlLockFinder.Tests.SessionCanvas.SessionCircleList
{
    public class SessionCircleList_when_Collides_sessionCircle: SessionCircleList_TestBase
    {
        [Test]
        public void It_should_return_true_when_collides_with_left_boundry()
        {
            sessionCircleList.MaxX = 100;
            sessionCircleList.MaxY = 100;

            var circle = new Mock<ISessionCircle>();
            circle.Setup(x => x.X).Returns(0);
            circle.Setup(x => x.Y).Returns(50);
            circle.Setup(x => x.Size).Returns(10);
            sessionCircleList.Add(circle.Object);

            var collides = sessionCircleList.Collides(circle.Object);

            collides.Should().BeTrue();
        }

        [Test]
        public void It_should_return_true_when_collides_with_right_boundry()
        {
            sessionCircleList.MaxX = 100;
            sessionCircleList.MaxY = 100;

            var circle = new Mock<ISessionCircle>();
            circle.Setup(x => x.X).Returns(100);
            circle.Setup(x => x.Y).Returns(50);
            circle.Setup(x => x.Size).Returns(10);
            sessionCircleList.Add(circle.Object);

            var collides = sessionCircleList.Collides(circle.Object);

            collides.Should().BeTrue();
        }

        [Test]
        public void It_should_return_true_when_collides_with_top_boundry()
        {
            sessionCircleList.MaxX = 100;
            sessionCircleList.MaxY = 100;

            var circle = new Mock<ISessionCircle>();
            circle.Setup(x => x.X).Returns(50);
            circle.Setup(x => x.Y).Returns(100);
            circle.Setup(x => x.Size).Returns(10);
            sessionCircleList.Add(circle.Object);

            var collides = sessionCircleList.Collides(circle.Object);

            collides.Should().BeTrue();
        }

        [Test]
        public void It_should_return_true_when_collides_with_bottom_boundry()
        {
            sessionCircleList.MaxX = 100;
            sessionCircleList.MaxY = 100;

            var circle = new Mock<ISessionCircle>();
            circle.Setup(x => x.X).Returns(50);
            circle.Setup(x => x.Y).Returns(0);
            circle.Setup(x => x.Size).Returns(10);
            sessionCircleList.Add(circle.Object);

            var collides = sessionCircleList.Collides(circle.Object);

            collides.Should().BeTrue();
        }

        [Test]
        public void It_should_return_false_when_not_collides_with_boundy_and_is_only_item()
        {
            sessionCircleList.MaxX = 100;
            sessionCircleList.MaxY = 100;

            var circle = new Mock<ISessionCircle>();
            circle.Setup(x => x.X).Returns(50);
            circle.Setup(x => x.Y).Returns(50);
            circle.Setup(x => x.Size).Returns(10);
            sessionCircleList.Add(circle.Object);

            var collides = sessionCircleList.Collides(circle.Object);

            collides.Should().BeFalse();
        }

        [Test]
        public void It_should_return_true_when_collides_with_left_of_other_circle()
        {
            sessionCircleList.MaxX = 100;
            sessionCircleList.MaxY = 100;

            var circle1 = new Mock<ISessionCircle>();
            circle1.Setup(x => x.X).Returns(30);
            circle1.Setup(x => x.Y).Returns(40);
            circle1.Setup(x => x.Size).Returns(10);
            sessionCircleList.Add(circle1.Object);

            var circle2 = new Mock<ISessionCircle>();
            circle2.Setup(x => x.X).Returns(40);
            circle2.Setup(x => x.Y).Returns(40);
            circle2.Setup(x => x.Size).Returns(10);
            sessionCircleList.Add(circle2.Object);

            var collides = sessionCircleList.Collides(circle2.Object);

            collides.Should().BeTrue();
        }

        [Test]
        public void It_should_return_true_when_collides_with_right_of_other_circle()
        {
            sessionCircleList.MaxX = 100;
            sessionCircleList.MaxY = 100;

            var circle1 = new Mock<ISessionCircle>();
            circle1.Setup(x => x.X).Returns(50);
            circle1.Setup(x => x.Y).Returns(40);
            circle1.Setup(x => x.Size).Returns(10);
            sessionCircleList.Add(circle1.Object);

            var circle2 = new Mock<ISessionCircle>();
            circle2.Setup(x => x.X).Returns(40);
            circle2.Setup(x => x.Y).Returns(40);
            circle2.Setup(x => x.Size).Returns(10);
            sessionCircleList.Add(circle2.Object);

            var collides = sessionCircleList.Collides(circle2.Object);

            collides.Should().BeTrue();
        }

        [Test]
        public void It_should_return_true_when_collides_with_bottom_of_other_circle()
        {
            sessionCircleList.MaxX = 100;
            sessionCircleList.MaxY = 100;

            var circle1 = new Mock<ISessionCircle>();
            circle1.Setup(x => x.X).Returns(40);
            circle1.Setup(x => x.Y).Returns(50);
            circle1.Setup(x => x.Size).Returns(10);
            sessionCircleList.Add(circle1.Object);

            var circle2 = new Mock<ISessionCircle>();
            circle2.Setup(x => x.X).Returns(40);
            circle2.Setup(x => x.Y).Returns(40);
            circle2.Setup(x => x.Size).Returns(10);
            sessionCircleList.Add(circle2.Object);

            var collides = sessionCircleList.Collides(circle2.Object);

            collides.Should().BeTrue();
        }

        [Test]
        public void It_should_return_true_when_collides_with_top_of_other_circle()
        {
            sessionCircleList.MaxX = 100;
            sessionCircleList.MaxY = 100;

            var circle1 = new Mock<ISessionCircle>();
            circle1.Setup(x => x.X).Returns(40);
            circle1.Setup(x => x.Y).Returns(30);
            circle1.Setup(x => x.Size).Returns(10);
            sessionCircleList.Add(circle1.Object);

            var circle2 = new Mock<ISessionCircle>();
            circle2.Setup(x => x.X).Returns(40);
            circle2.Setup(x => x.Y).Returns(40);
            circle2.Setup(x => x.Size).Returns(10);
            sessionCircleList.Add(circle2.Object);

            var collides = sessionCircleList.Collides(circle2.Object);

            collides.Should().BeTrue();
        }

        [Test]
        public void It_should_return_false_when_never_collides_with_other_circle()
        {
            sessionCircleList.MaxX = 100;
            sessionCircleList.MaxY = 100;

            var circle1 = new Mock<ISessionCircle>();
            circle1.Setup(x => x.X).Returns(70);
            circle1.Setup(x => x.Y).Returns(70);
            circle1.Setup(x => x.Size).Returns(10);
            sessionCircleList.Add(circle1.Object);

            var circle2 = new Mock<ISessionCircle>();
            circle2.Setup(x => x.X).Returns(40);
            circle2.Setup(x => x.Y).Returns(40);
            circle2.Setup(x => x.Size).Returns(10);
            sessionCircleList.Add(circle2.Object);

            var collides = sessionCircleList.Collides(circle2.Object);

            collides.Should().BeFalse();
        }

    }
}