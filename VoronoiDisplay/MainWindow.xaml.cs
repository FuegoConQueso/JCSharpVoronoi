using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using JCSharpVoronoi;

namespace VoronoiDisplay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private bool isGenerated = false;
        public int PointCount { get; set; } = 50;
        public float DiagramWidth => (int)Canvas.ActualWidth;
        public float DiagramHeight => (int)Canvas.ActualHeight;

        public double SeedNumber { get; set; } = 0;

        public ICommand DrawCommand { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            DrawCommand = new Command(param => GenerateAndDraw());

            Canvas.SizeChanged += (sender, args) =>
            {
                OnPropertyChanged(nameof(DiagramHeight));
                OnPropertyChanged(nameof(DiagramWidth));
            };

            Canvas.MouseLeftButtonUp += (sender, e) =>
            {
                System.Windows.Point mousePos = e.GetPosition((IInputElement)sender);
                FindRegion(new Point(mousePos.X, mousePos.Y));
            };

        }

        private void GenerateAndDraw()
        {
            Canvas.Children.Clear();

            var points = JCVGenerator.GeneratePoints(PointCount, DiagramWidth, DiagramHeight, (int)SeedNumber);
            JCVDiagram d = Voronoi.JCVDiagramGenerate(points, DiagramWidth, DiagramHeight);

            DrawEdges(d.edges);

            DrawSiteCenters(d.sites);
        }

        private void DrawEdges(IEnumerable<JCVEdge> edges)
        {
            foreach (var edge in edges)
            {
                var line = new Line();
                line.Stroke = Brushes.DarkViolet;

                line.StrokeThickness = 1;

                line.X1 = edge.Points[0].X;
                line.Y1 = edge.Points[0].Y;
                line.X2 = edge.Points[1].X;
                line.Y2 = edge.Points[1].Y;
                if (line.X1 < 0 || line.X2 < 0 || line.Y1 < 0 || line.Y2 < 0)
                {
                    Debug.WriteLine("(" + line.X1 + "," + line.Y1 + ") - (" + line.X2 + "," + line.Y2 + ")");
                }

                Canvas.Children.Add(line);
            }
        }

        private void DrawSiteCenters(IEnumerable<JCVSite> sites)
        {
            foreach (var site in sites)
            {
                var myEllipse = new Ellipse();
                myEllipse.Fill = System.Windows.Media.Brushes.Red;
                myEllipse.HorizontalAlignment = HorizontalAlignment.Left;
                myEllipse.VerticalAlignment = VerticalAlignment.Top;
                myEllipse.Width = 2;
                myEllipse.Height = 2;
                var ellipseX = site.X - 0.5 * myEllipse.Height;
                var ellipseY = site.Y - 0.5 * myEllipse.Width;
                myEllipse.Margin = new Thickness(ellipseX, ellipseY, 0, 0);

                Canvas.Children.Add(myEllipse);
            }
        }

        private void FindRegion(Point point)
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public class Command : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public Command(Action<object> execute)
            : this(execute, param => true)
        {
        }

        public Command(Action<object> execute, Func<object, bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return (_canExecute == null) || _canExecute(parameter);
        }
    }
}
