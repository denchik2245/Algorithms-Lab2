using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.Wpf;
using Logic;

namespace FractalApp
{
    public partial class MainWindow : Window
    {
        private IFractal _fractal;
        private HanoiTowers _hanoiTowers;
        private int _diskCount = 5;
        private readonly double _rodWidth = 10;
        private readonly double _rodHeight = 200;
        private readonly double _diskHeight = 20;
        private double[] _rodPositions;
        private int _currentMoveIndex = 0;
        private List<string> _moves;
        private bool _isAutoMode = false;

        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void SetVisibility(UIElement element, Visibility visibility)
        {
            if (element != null)
            {
                element.Visibility = visibility;
            }
        }
        
        private void FractalRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            SetVisibility(DisplayCanvas, Visibility.Visible);
            SetVisibility(MyChart, Visibility.Collapsed);
            SetVisibility(FractalControls, Visibility.Visible);
            SetVisibility(RecursionDepthPanel, Visibility.Visible);
            SetVisibility(HanoiControls, Visibility.Collapsed);
            SetVisibility(GraphControls, Visibility.Collapsed);
            SetVisibility(ButtonCalculation, Visibility.Visible);
            SetVisibility(StartHanoiButton, Visibility.Collapsed);
            SetVisibility(StopButton, Visibility.Collapsed);
            SetVisibility(StepBackButton, Visibility.Collapsed);
            SetVisibility(StepForwardButton, Visibility.Collapsed);
            SetVisibility(StartCalculationButton, Visibility.Collapsed); 

            DisplayCanvas?.Children.Clear();
        }

        private void HanoiRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            SetVisibility(DisplayCanvas, Visibility.Visible);
            SetVisibility(MyChart, Visibility.Collapsed);
            SetVisibility(FractalControls, Visibility.Collapsed);
            SetVisibility(RecursionDepthPanel, Visibility.Collapsed);
            SetVisibility(HanoiControls, Visibility.Visible);
            SetVisibility(HanoiButtonsPanel, Visibility.Visible);
            SetVisibility(GraphControls, Visibility.Collapsed);
            SetVisibility(StartHanoiButton, Visibility.Visible);
            SetVisibility(ButtonCalculation, Visibility.Collapsed);
            SetVisibility(StartCalculationButton, Visibility.Collapsed); 
            SetVisibility(StopButton, Visibility.Visible);
            SetVisibility(StepBackButton, Visibility.Visible);
            SetVisibility(StepForwardButton, Visibility.Visible);

            DisplayCanvas?.Children.Clear();
            DrawHanoiRods();
            DrawDisks(_diskCount);
        }


        private void GraphsRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            SetVisibility(DisplayCanvas, Visibility.Collapsed);
            SetVisibility(MyChart, Visibility.Visible);
            SetVisibility(FractalControls, Visibility.Collapsed);
            SetVisibility(RecursionDepthPanel, Visibility.Collapsed);
            SetVisibility(HanoiControls, Visibility.Collapsed);
            SetVisibility(HanoiButtonsPanel, Visibility.Collapsed);
            SetVisibility(GraphControls, Visibility.Visible);
            SetVisibility(StartCalculationButton, Visibility.Visible);
            SetVisibility(ButtonCalculation, Visibility.Collapsed);
            SetVisibility(StartHanoiButton, Visibility.Collapsed);
            SetVisibility(StopButton, Visibility.Collapsed);

            ConfigureChart();
        }
        
        private void UpdateButtonStates()
        {
            if (_isAutoMode)
            {
                StepForwardButton.IsEnabled = false;
                StepBackButton.IsEnabled = false;
                StartHanoiButton.IsEnabled = false;
                StopButton.IsEnabled = true;
            }
            else
            {
                StepForwardButton.IsEnabled = _moves != null && _currentMoveIndex < _moves.Count;
                StepBackButton.IsEnabled = _moves != null && _currentMoveIndex > 0;
                StartHanoiButton.IsEnabled = true;
                StopButton.IsEnabled = false;
            }
        }
        
        private void ConfigureChart()
        {
            MyChart.AxisX.Clear();
            MyChart.AxisX.Add(new Axis
            {
                Title = "Количество колец",
                LabelFormatter = value => value.ToString("F0"),
                MinValue = 0
            });

            MyChart.AxisY.Clear();
            MyChart.AxisY.Add(new Axis
            {
                Title = "Время выполнения (мс)",
                LabelFormatter = value => value.ToString("F3"),
                MinValue = 0
            });

            MyChart.AnimationsSpeed = TimeSpan.FromMilliseconds(300);
            MyChart.Zoom = ZoomingOptions.Xy;
            MyChart.LegendLocation = LegendLocation.None;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            int maxDisks = int.Parse(MaxDisksTextBox.Text);
            int runs = int.Parse(RunsTextBox.Text);

            List<double> times = new List<double>();
            List<double> disks = new List<double>();

            HanoiTowers hanoi = new HanoiTowers();

            for (int n = 1; n <= maxDisks; n++) // Проверяем до maxDisks включительно
            {
                double totalTime = 0;

                for (int run = 0; run < runs; run++)
                {
                    var watch = System.Diagnostics.Stopwatch.StartNew();

                    hanoi.Solve(n, "A", "B", "C");

                    // Искусственная задержка для визуализации на графике
                    System.Threading.Thread.Sleep(50);

                    watch.Stop();
                    totalTime += watch.ElapsedMilliseconds;
                }

                disks.Add(n); // Добавляем количество колец
                times.Add(totalTime / runs); // Вычисляем среднее время выполнения
            }

            // Обновляем график
            MyChart.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Время выполнения",
                    Values = new ChartValues<double>(times),
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 5
                }
            };

            // Обновляем ось X для отображения количества дисков
            MyChart.AxisX.Clear();
            MyChart.AxisX.Add(new Axis
            {
                Title = "Количество колец",
                LabelFormatter = value => value.ToString("F0"),
                MinValue = 1,
                MaxValue = maxDisks + 1, // Добавляем 1, чтобы гарантировать включение последнего значения
            });

            // Ось Y уже настроена на отображение времени в миллисекундах
            MyChart.AxisY[0].Title = "Время выполнения (мс)";
        }
        
        private async void StepForwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isAutoMode)
            {
                MessageBox.Show("Невозможно выполнить шаг вперед в автоматическом режиме.");
                return;
            }

            if (_moves != null && _currentMoveIndex < _moves.Count)
            {
                var parsedMove = ParseMove(_moves[_currentMoveIndex]);
                if (parsedMove.HasValue)
                {
                    await MoveDisk(parsedMove.Value.DiskNumber, parsedMove.Value.FromRod, parsedMove.Value.ToRod);
                    _currentMoveIndex++;
                    UpdateButtonStates();
                }
            }
            else
            {
                MessageBox.Show("Достигнут конец списка перемещений.");
            }
        }

        private async void StepBackButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isAutoMode)
            {
                MessageBox.Show("Невозможно выполнить шаг назад в автоматическом режиме.");
                return;
            }

            if (_moves != null && _currentMoveIndex > 0)
            {
                _currentMoveIndex--;
                var parsedMove = ParseMove(_moves[_currentMoveIndex]);
                if (parsedMove.HasValue)
                {
                    await MoveDisk(parsedMove.Value.DiskNumber, parsedMove.Value.ToRod, parsedMove.Value.FromRod);
                    UpdateButtonStates();
                }
            }
            else
            {
                MessageBox.Show("Вы находитесь в начале списка перемещений.");
            }
        }
        
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isAutoMode)
            {
                _isAutoMode = false;
                UpdateButtonStates();
            }
        }
        
        private void GenerateFractalButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(RecursionDepth.Text, out int depth) && depth >= 0)
            {
                _fractal = FractalSelector.SelectedIndex switch
                {
                    0 => new SierpinskiCarpet(DisplayCanvas),
                    1 => new LeviCurve(DisplayCanvas),
                    _ => null
                };

                if (_fractal == null)
                {
                    MessageBox.Show("Выберите фрактал.");
                    return;
                }

                DisplayCanvas?.Children.Clear();
                _fractal.DrawFractal(depth);
            }
            else
            {
                MessageBox.Show("Введите корректную глубину рекурсии.");
            }
        }
        
        private async void StartHanoiButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(DiskCount.Text, out _diskCount) && _diskCount > 0)
            {
                DisplayCanvas.Children.Clear();
                DrawHanoiRods();
                DrawDisks(_diskCount);

                _hanoiTowers = new HanoiTowers();
                _hanoiTowers.Solve(_diskCount, "A", "B", "C");

                _moves = _hanoiTowers.GetMoves().ToList();
                _currentMoveIndex = 0;
                _isAutoMode = true;

                UpdateButtonStates();

                await DisplayMoves();

                _isAutoMode = false;
                UpdateButtonStates();
            }
            else
            {
                MessageBox.Show("Введите корректное количество дисков.");
            }
        }
        
        private void DrawHanoiRods()
        {
            if (DisplayCanvas == null) return;

            DisplayCanvas.Children.Clear();
            double canvasCenter = DisplayCanvas.Width / 2;
            double rodSpacing = 150;

            _rodPositions = new double[3];
            for (int i = -1; i <= 1; i++)
            {
                var rod = new Rectangle
                {
                    Width = _rodWidth,
                    Height = _rodHeight,
                    Fill = Brushes.Black
                };
                Canvas.SetLeft(rod, canvasCenter + i * rodSpacing - _rodWidth / 2);
                Canvas.SetTop(rod, DisplayCanvas.Height - _rodHeight - 20);
                DisplayCanvas.Children.Add(rod);
                
                var baseRod = new Rectangle
                {
                    Width = _rodWidth * 6,
                    Height = _rodWidth * 2,
                    Fill = Brushes.SaddleBrown 
                };
                Canvas.SetLeft(baseRod, canvasCenter + i * rodSpacing - (_rodWidth * 6) / 2);
                Canvas.SetTop(baseRod, DisplayCanvas.Height - 20);
                DisplayCanvas.Children.Add(baseRod);

                _rodPositions[i + 1] = canvasCenter + i * rodSpacing;
            }
        }
        
        private void DrawDisks(int diskCount)
        {
            if (DisplayCanvas == null) return;

            double rodX = _rodPositions[0];
            double baseY = DisplayCanvas.Height - 20;
            double maxDiskWidth = 130;
            double minDiskWidth = 50;
            double diskWidthStep = (maxDiskWidth - minDiskWidth) / (diskCount - 1);

            Brush[] diskColors = {
                Brushes.Red,
                Brushes.Brown,
                Brushes.Yellow,
                Brushes.Green,
                Brushes.LightGreen,
                Brushes.LightBlue,
                Brushes.Blue
            };

            for (int i = 0; i < diskCount; i++)
            {
                double diskWidth = maxDiskWidth - i * diskWidthStep;

                var disk = new Ellipse
                {
                    Width = diskWidth,
                    Height = _diskHeight,
                    Fill = diskColors[i % diskColors.Length]
                };

                Canvas.SetLeft(disk, rodX - diskWidth / 2);
                Canvas.SetTop(disk, baseY - (i + 1) * _diskHeight);

                int diskNumber = diskCount - i;

                disk.Tag = new DiskInfo { CurrentRod = 0, PositionOnRod = i, DiskNumber = diskNumber };
                DisplayCanvas.Children.Add(disk);
            }
        }
        
        private async Task DisplayMoves()
        {
            while (_currentMoveIndex < _moves.Count && _isAutoMode)
            {
                var parsedMove = ParseMove(_moves[_currentMoveIndex]);
                if (parsedMove.HasValue)
                {
                    await MoveDisk(parsedMove.Value.DiskNumber, parsedMove.Value.FromRod, parsedMove.Value.ToRod);
                    _currentMoveIndex++;
                    UpdateButtonStates();
                    await Task.Delay(150);
                }
                else
                {
                    break;
                }
            }
        }
        
        private Task MoveDisk(int diskNumber, int fromRod, int toRod)
        {
            return Application.Current.Dispatcher.InvokeAsync(() =>
            {
                foreach (UIElement element in DisplayCanvas.Children)
                {
                    if (element is Ellipse disk && disk.Tag is DiskInfo info && info.CurrentRod == fromRod && info.DiskNumber == diskNumber)
                    {
                        int newPosition = GetNextAvailablePosition(toRod);

                        info.CurrentRod = toRod;
                        info.PositionOnRod = newPosition;

                        double rodX = _rodPositions[toRod];
                        double baseY = DisplayCanvas.Height - 20;
                        double diskY = baseY - (newPosition + 1) * _diskHeight;

                        Canvas.SetLeft(disk, rodX - disk.Width / 2);
                        Canvas.SetTop(disk, diskY);

                        break;
                    }
                }
            }).Task;
        }
        
        private int GetNextAvailablePosition(int rod)
        {
            int maxPosition = -1;
            foreach (UIElement element in DisplayCanvas.Children)
            {
                if (element is Ellipse disk && disk.Tag is DiskInfo info && info.CurrentRod == rod)
                {
                    if (info.PositionOnRod > maxPosition)
                    {
                        maxPosition = info.PositionOnRod;
                    }
                }
            }
            return maxPosition + 1;
        }
        
        private (int DiskNumber, int FromRod, int ToRod)? ParseMove(string move)
        {
            string[] parts = move.Split(' ');
            if (parts.Length == 7 && int.TryParse(parts[2], out int disk))
            {
                int fromRod = parts[4] == "A" ? 0 : (parts[4] == "B" ? 1 : 2);
                int toRod = parts[6] == "A" ? 0 : (parts[6] == "B" ? 1 : 2);
                return (disk, fromRod, toRod);
            }
            return null;
        }

        private class DiskInfo
        {
            public int CurrentRod { get; set; }
            public int PositionOnRod { get; set; }
            public int DiskNumber { get; set; }
        }

    }
}