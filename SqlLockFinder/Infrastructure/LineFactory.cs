using System.Windows.Media;
using System.Windows.Shapes;

namespace SqlLockFinder.Infrastructure
{
    public interface ILineFactory
    {
        object Create(int x1, int y1, int x2, int y2, Color color);
    }

    public class LineFactory : ILineFactory
    {
        public object Create(int x1, int y1, int x2, int y2, Color color)
        {
            return new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = new SolidColorBrush(color),
            };
        }
    }
}