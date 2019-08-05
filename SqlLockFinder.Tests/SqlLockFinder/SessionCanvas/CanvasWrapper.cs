using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SqlLockFinder.SessionCanvas
{
    public interface ICanvasWrapper
    {
        void Add(UIElement uiElement, int zIndex);
        void Remove(UIElement uiElement);
        void RemoveAll<T>() where T: UIElement;
        void SetPosition(UIElement uiElement, int x, int y);
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

        public void Add(UIElement uiElement, int zIndex)
        {
            canvas.Children.Add(uiElement);
            Canvas.SetZIndex(uiElement, zIndex);
        }

        public void Remove(UIElement uiElement)
        {
            canvas.Children.Remove(uiElement);
        }

        public void RemoveAll<T>() where T: UIElement
        {
            foreach (var line in canvas.Children.OfType<T>().ToList())
            {
                canvas.Children.Remove(line);
            }
        }

        public void SetPosition(UIElement uiElement, int x, int y)
        {
            Canvas.SetLeft(uiElement, x);
            Canvas.SetTop(uiElement, y);
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