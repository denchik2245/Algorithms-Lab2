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
        
        // Радиокнопка "Отрисовка фракталов"
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

            // Скрыть только радиокнопки типа графика
            SetVisibility(GraphTypeRadioButtons, Visibility.Collapsed);
            
            DisplayCanvas?.Children.Clear();
        }

        // Радиокнопка "Ханойские башни"
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

            // Скрыть только радиокнопки типа графика
            SetVisibility(GraphTypeRadioButtons, Visibility.Collapsed);

            DisplayCanvas?.Children.Clear();
            DrawHanoiRods();
            DrawDisks(_diskCount);
        }

        // Радиокнопка "Графики"
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

            // Показать радиокнопки типа графика
            SetVisibility(GraphTypeRadioButtons, Visibility.Visible);

            // Установить по умолчанию "Время визуализации"
            VisualizationTimeRadioButton.IsChecked = true;

            ConfigureChart();
        }
        
        // Обработчик для новых радиокнопок "Тип графика"
        private void GraphTypeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (VisualizationTimeRadioButton.IsChecked == true)
            {
                ConfigureChartVisualizationTime();
            }
            else if (AlgorithmTimeRadioButton.IsChecked == true)
            {
                ConfigureChartAlgorithmTime();
            }
            else if (StepsCountRadioButton.IsChecked == true)
            {
                ConfigureChartStepsCount();
            }
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
        
        //Кнопка "Вперед"
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

        //Кнопка "Назад"
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
        
        //Кнопка "Стоп"
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isAutoMode)
            {
                _isAutoMode = false;
                UpdateButtonStates();
            }
        }
        
        //Генерация фракталов
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
        
        //Генерация Ханойских башен
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
        
        //График
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
                Title = "Время выполнения (сек)",  // Время в секундах
                LabelFormatter = value => (value / 1000).ToString("F3"),  // Переводим миллисекунды в секунды, отображаем с 3 знаками после запятой
                MinValue = 0
            });

            MyChart.AnimationsSpeed = TimeSpan.FromMilliseconds(300);
            MyChart.Zoom = ZoomingOptions.Xy;
            MyChart.LegendLocation = LegendLocation.None;
        }

        // Метод настройки графика для "Время визуализации"
        private void ConfigureChartVisualizationTime()
        {
            ConfigureChart();
            MyChart.AxisY[0].Title = "Время визуализации (сек)";
            MyChart.Series.Clear();
            
            MyChart.Series.Add(new LineSeries
            {
                Title = "Время визуализации",
                Values = new ChartValues<double>(),
                PointGeometry = DefaultGeometries.Circle,
                PointGeometrySize = 5,
                Stroke = System.Windows.Media.Brushes.Blue,
                Fill = new System.Windows.Media.SolidColorBrush(Color.FromArgb(50, 0, 0, 255)),
            });
            
            // Настраиваем форматирование осей: переводим миллисекунды в секунды
            MyChart.AxisY[0].LabelFormatter = value => (value / 1000).ToString("F3");  // Формат отображения в секундах
        }
        
        // Метод настройки графика для "Время алгоритма"
        private void ConfigureChartAlgorithmTime()
        {
            ConfigureChart();
            MyChart.AxisY[0].Title = "Время алгоритма (мс)";  // Оставляем время в миллисекундах
            MyChart.Series.Clear();
    
            MyChart.Series.Add(new LineSeries
            {
                Title = "Время алгоритма",
                Values = new ChartValues<double>(),  // Проверьте, что сюда подаются корректные данные
                PointGeometry = DefaultGeometries.Circle,
                PointGeometrySize = 5,
                Stroke = System.Windows.Media.Brushes.Green,
                Fill = new System.Windows.Media.SolidColorBrush(Color.FromArgb(50, 0, 255, 0)),
            });
    
            // Оставляем формат оси Y без деления на 1000
            MyChart.AxisY[0].LabelFormatter = value => value.ToString("F3");  // Отображаем миллисекунды с тремя знаками после запятой
        }
        
        // Метод настройки графика для "Количество шагов"
        private void ConfigureChartStepsCount()
        {
            ConfigureChart();
            MyChart.AxisY[0].Title = "Количество шагов";
            MyChart.Series.Clear();
            
            MyChart.Series.Add(new LineSeries
            {
                Title = "Количество шагов",
                Values = new ChartValues<double>(),
                PointGeometry = DefaultGeometries.Circle,
                PointGeometrySize = 5,
                Stroke = System.Windows.Media.Brushes.Red,
                Fill = new System.Windows.Media.SolidColorBrush(Color.FromArgb(50, 255, 0, 0)),
            });
            
            MyChart.AxisY[0].LabelFormatter = value => value.ToString("F0");
        }
        
        // Кнопка "Начать расчет"
        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(MaxDisksTextBox.Text, out int maxDisks) || maxDisks <= 0)
            {
                MessageBox.Show("Пожалуйста, введите корректное положительное целое число для максимального количества колец.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (VisualizationTimeRadioButton.IsChecked == true)
            {
                List<double> visualizationTimes = await MeasureVisualizationTime(maxDisks);
                UpdateChartVisualizationTime(visualizationTimes);
            }
            else if (AlgorithmTimeRadioButton.IsChecked == true)
            {
                List<double> algorithmTimes = MeasureAlgorithmTime(maxDisks);
                UpdateChartAlgorithmTime(algorithmTimes);
            }
            else if (StepsCountRadioButton.IsChecked == true)
            {
                List<int> stepsCounts = MeasureStepsCount(maxDisks);
                UpdateChartStepsCount(stepsCounts);
            }
        }
        
        // Метод обновления графика для "Время визуализации"
        private void UpdateChartVisualizationTime(List<double> visualizationTimes)
        {
            if (MyChart.Series.Count > 0 && MyChart.Series[0] is LineSeries visualizationSeries)
            {
                visualizationSeries.Values.Clear();
                foreach (var time in visualizationTimes)
                {
                    visualizationSeries.Values.Add(time);
                }
            }

            // Настройка меток оси X
            MyChart.AxisX[0].Labels = Enumerable.Range(1, visualizationTimes.Count).Select(x => x.ToString()).ToArray();
        }
        
        // Метод обновления графика для "Время алгоритма"
        private void UpdateChartAlgorithmTime(List<double> algorithmTimes)
        {
            if (MyChart.Series.Count > 0 && MyChart.Series[0] is LineSeries algorithmSeries)
            {
                algorithmSeries.Values.Clear();
                foreach (var time in algorithmTimes)
                {
                    algorithmSeries.Values.Add(time);
                }
            }

            // Настройка меток оси X
            MyChart.AxisX[0].Labels = Enumerable.Range(1, algorithmTimes.Count).Select(x => x.ToString()).ToArray();
        }
        
        // Метод обновления графика для "Количество шагов"
        private void UpdateChartStepsCount(List<int> stepsCounts)
        {
            if (MyChart.Series.Count > 0 && MyChart.Series[0] is LineSeries stepsSeries)
            {
                stepsSeries.Values.Clear();
                foreach (var steps in stepsCounts)
                {
                    stepsSeries.Values.Add((double)steps); // Преобразование int в double
                }
            }

            // Настройка меток оси X
            MyChart.AxisX[0].Labels = Enumerable.Range(1, stepsCounts.Count).Select(x => x.ToString()).ToArray();
        }
        
        // Метод измерения времени алгоритма (5 запусков, среднее время)
        private List<double> MeasureAlgorithmTime(int maxDisks)
        {
            List<double> algorithmTimes = new List<double>();
            HanoiTowers hanoi = new HanoiTowers();
            int numberOfRuns = 10;  // Количество запусков

            for (int n = 1; n <= maxDisks; n++)
            {
                double totalElapsedTime = 0;

                // Проводим 5 запусков для текущего количества дисков
                for (int run = 0; run < numberOfRuns; run++)
                {
                    var watch = System.Diagnostics.Stopwatch.StartNew();

                    hanoi.Solve(n, "A", "B", "C");

                    watch.Stop();
                    totalElapsedTime += watch.Elapsed.TotalMilliseconds;
                }

                // Вычисляем среднее время выполнения и добавляем его в список
                double averageTime = totalElapsedTime / numberOfRuns;
                algorithmTimes.Add(averageTime);
            }

            return algorithmTimes;
        }

        // Метод измерения времени визуализации (один запуск)
        private async Task<List<double>> MeasureVisualizationTime(int maxDisks)
        {
            List<double> visualizationTimes = new List<double>();

            for (int n = 1; n <= maxDisks; n++)
            {
                DisplayCanvas.Children.Clear();
                DrawHanoiRods();
                DrawDisks(n);

                _hanoiTowers = new HanoiTowers();
                _hanoiTowers.Solve(n, "A", "B", "C");

                _moves = _hanoiTowers.GetMoves().ToList();
                _currentMoveIndex = 0;
                _isAutoMode = true;

                UpdateButtonStates();

                var watch = System.Diagnostics.Stopwatch.StartNew();

                await DisplayMoves();

                watch.Stop();

                _isAutoMode = false;
                UpdateButtonStates();

                double elapsedMs = watch.Elapsed.TotalMilliseconds;
                visualizationTimes.Add(elapsedMs);

                await Task.Delay(500);
            }

            return visualizationTimes;
        }

        // Метод измерения количества шагов (один запуск)
        private List<int> MeasureStepsCount(int maxDisks)
        {
            List<int> stepsCounts = new List<int>();
            HanoiTowers hanoi = new HanoiTowers();

            for (int n = 1; n <= maxDisks; n++)
            {
                hanoi.Solve(n, "A", "B", "C");
                int steps = hanoi.GetMoveCount();
                stepsCounts.Add(steps);
            }

            return stepsCounts;
        }
        
        //Отрисовка стержней
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
        
        //Отрисовка дисков
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
                    await Task.Delay(10);
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