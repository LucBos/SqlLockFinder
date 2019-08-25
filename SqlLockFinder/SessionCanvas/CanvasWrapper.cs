using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SqlLockFinder.SessionCanvas
{
    public interface ICanvasWrapper
    {
        void Add(object uiElement, int zIndex);
        void Remove(object uiElement);
        void RemoveAll<T>();
        void SetPosition(object uiElement, int x, int y);
        int ActualWidth { get; }
        int ActualHeight { get; }
        void TrackMouse(SessionDetail.SessionOverview sessionOverview);
        void SetColor(Color color);
    }

    public class CanvasWrapper : ICanvasWrapper
    {
        private readonly Canvas canvas;

        public CanvasWrapper(Canvas canvas)
        {
            this.canvas = canvas;
        }

        public void Add(object uiElement, int zIndex)
        {
            canvas.Children.Add(uiElement as UIElement);
            Canvas.SetZIndex(uiElement as UIElement, zIndex);
        }

        public void Remove(object uiElement)
        {
            canvas.Children.Remove(uiElement as UIElement);
        }

        public void RemoveAll<T>()
        {
            foreach (var line in canvas.Children.OfType<T>().ToList())
            {
                canvas.Children.Remove(line as UIElement);
            }
        }

        public void SetPosition(object uiElement, int x, int y)
        {
            Canvas.SetLeft(uiElement as UIElement, x);
            Canvas.SetTop(uiElement as UIElement, y);
        }
        public void TrackMouse(SessionDetail.SessionOverview sessionOverview)
        {
            var sizeX = sessionOverview.Width;
            var sizeY = sessionOverview.Height;

            var position = Mouse.GetPosition(canvas);
            if (position.X + sizeX > canvas.ActualWidth)
            {
                position.X = canvas.ActualWidth - sizeX;
            }
            if (position.Y + sizeY > canvas.ActualHeight)
            {
                position.Y = canvas.ActualHeight - sizeY;
            }

            SetPosition(sessionOverview, (int) position.X, (int) position.Y);
        }

        public void SetColor(Color color)
        {
            canvas.Background = new SolidColorBrush(color);
        }

        public int ActualWidth => (int)canvas.ActualWidth;
        public int ActualHeight => (int) canvas.ActualHeight;
       
    }
}