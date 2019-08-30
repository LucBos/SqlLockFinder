using Moq;
using NUnit.Framework;
using SqlLockFinder.SessionCanvas;

namespace SqlLockFinder.Tests.SessionCanvas.SessionCircleList
{
    public class SessionCircleList_when_DeselectAll: SessionCircleList_TestBase
    {
        [Test]
        public void It_should_set_the_selected_flag_on_all_circles_to_false()
        {
            var circle1 = new Mock<ISessionCircle>();
            var circle2 = new Mock<ISessionCircle>();
            var circle3 = new Mock<ISessionCircle>();

            sessionCircleList.Add(circle1.Object);
            sessionCircleList.Add(circle2.Object);
            sessionCircleList.Add(circle3.Object);

            sessionCircleList.DeselectAll();

            circle1.VerifySet(x => x.Selected = false);
            circle2.VerifySet(x => x.Selected = false);
            circle3.VerifySet(x => x.Selected = false);
        }
    }
}