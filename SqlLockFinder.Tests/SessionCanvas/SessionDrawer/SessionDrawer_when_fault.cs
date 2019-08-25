using NUnit.Framework;

namespace SqlLockFinder.Tests.SessionCanvas.SessionDrawer
{
    public class SessionDrawer_when_fault: SessionDrawer_TestBase
    {
        [Test]
        public void It_should_update_the_canvas_color()
        {
            sessionDrawer.Fault();

            canvasWrapper.Verify(x => x.SetColor(SqlLockFinder.SessionCanvas.SessionDrawer.FaultColor));
        }
    }
}