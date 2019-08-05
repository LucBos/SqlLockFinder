using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using SqlLockFinder.ActivityMonitor;
using SqlLockFinder.Infrastructure;

namespace SqlLockFinder.SessionCanvas
{
    public class SessionCircle
    {
        private int size;
        private SessionDto session;
        public Ellipse Ellipse { get; }
        private Canvas canvas;
        private TextBlock textBlock;
        private bool selected;
        private Ellipse selectedEllipse;

        public event Action<SessionCircle> MouseOver;
        public event Action<SessionCircle> MouseLeave;
        public event Action<SessionCircle> MouseDown;

        public SessionCircle()
        {
            Ellipse = new Ellipse {Tag = this};
            selectedEllipse = new Ellipse();
            canvas = new Canvas();
            canvas.Children.Add(Ellipse);
            canvas.Children.Add(selectedEllipse);

            UiElement = canvas;

            UiElement.MouseEnter += (sender, e) => MouseOver?.Invoke(this);
            UiElement.MouseLeave += (sender, e) => MouseLeave?.Invoke(this);
            UiElement.MouseDown += (sender, e) => MouseDown?.Invoke(this);
        }

        public SessionDto Session
        {
            get => session;
            set
            {
                session = value;
                CreateText();
                SetColorByStatus();
            }
        }

        private void SetColorByStatus()
        {
            switch (session.Status.Trim().ToLower())
            {
                case "suspended":
                    Ellipse.Fill = new SolidColorBrush(Colors.DarkOrange);
                    break;
                case "sleeping":
                    Ellipse.Fill = new SolidColorBrush(Colors.Gray);
                    break;
                case "background":
                    Ellipse.Fill = new SolidColorBrush(Colors.Lavender);
                    break;
                default:
                    Ellipse.Fill = new SolidColorBrush(Colors.Green);
                    break;
            }
        }

        private void CreateText()
        {
            var fontSize = (size == 0 ? 20 : size) / 3;
            if (textBlock == null)
            {
                textBlock = new TextBlock();
                canvas.Children.Add(textBlock);
            }

            textBlock.Text = session.SPID.ToString();
            textBlock.FontSize = fontSize;
            textBlock.Width = size;
            textBlock.TextAlignment = TextAlignment.Center;
            textBlock.Foreground = new SolidColorBrush(Colors.White);
            Canvas.SetLeft(textBlock, 0);
            Canvas.SetTop(textBlock, (int) ((size / 2) - (fontSize / 2)));
            Canvas.SetZIndex(textBlock, 10);
        }

        public UIElement UiElement { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public int SpeedX { get; set; }
        public int SpeedY { get; set; }
        public DateTime LastSpeedUpdate { get; set; }

        public int Size
        {
            get => size;
            set
            {
                size = value;
                CreateEllipse();
            }
        }

        public bool Selected
        {
            get => selected;
            set
            {
                selected = value;
                if (selected)
                {
                    selectedEllipse.Fill = new SolidColorBrush(Colors.White);
                }
                else
                {
                    selectedEllipse.Fill = new SolidColorBrush(Colors.Transparent);
                }
            }
        }

        private void CreateEllipse()
        {
            canvas.Width = size;
            canvas.Height = size;
            Ellipse.Width = size;
            Ellipse.Height = size;
            selectedEllipse.Width = size + 4;
            selectedEllipse.Height = size + 4;
            Canvas.SetLeft(selectedEllipse, -2);
            Canvas.SetTop(selectedEllipse, -2);
            Canvas.SetZIndex(Ellipse, 1);
            Canvas.SetZIndex(selectedEllipse, 0);
        }

        public void Move()
        {
            X += SpeedX;
            Y += SpeedY;
        }

        public void Revert()
        {
            SpeedX = SpeedX == 0 ? GlobalRandom.Instance.Next(-1, 2) : -SpeedX;
            SpeedY = SpeedY == 0 ? GlobalRandom.Instance.Next(-1, 2) : -SpeedY;
        }
    }
}