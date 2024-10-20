using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
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

        public MainWindow()
        {
            InitializeComponent();
        }

        // Обработчик переключения на "Отрисовку фракталов"
        private void FractalRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            // Убедимся, что элементы не null перед использованием
            if (FractalControls != null)
            {
                FractalControls.Visibility = Visibility.Visible;
            }
            if (RecursionDepthPanel != null)
            {
                RecursionDepthPanel.Visibility = Visibility.Visible;
            }
            if (HanoiControls != null)
            {
                HanoiControls.Visibility = Visibility.Collapsed;
            }
            if (HanoiButtonsPanel != null)
            {
                HanoiButtonsPanel.Visibility = Visibility.Collapsed; // Скрываем кнопки "Шаг вперед" и "Шаг назад"
            }
            if (DisplayCanvas != null)
            {
                DisplayCanvas.Children.Clear();
            }
        }

        // Обработчик переключения на "Ханойские башни"
        private void HanoiRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (FractalControls != null)
            {
                FractalControls.Visibility = Visibility.Collapsed;
            }
            if (RecursionDepthPanel != null)
            {
                RecursionDepthPanel.Visibility = Visibility.Collapsed;
            }

            // Показываем элементы для Ханойских башен
            if (HanoiControls != null)
            {
                HanoiControls.Visibility = Visibility.Visible; // Поле для ввода количества дисков
            }
            if (HanoiButtonsPanel != null)
            {
                HanoiButtonsPanel.Visibility = Visibility.Visible; // Кнопки "Шаг вперед" и "Шаг назад"
            }

            if (DisplayCanvas != null)
            {
                DisplayCanvas.Children.Clear();
            }
            DrawHanoiRods();
            DrawDisks(_diskCount); // Используется количество дисков, заданное ранее
        }

        // Обработчик кнопки "Шаг вперед"
        private void StepForwardButton_Click(object sender, RoutedEventArgs e)
        {
            // Временная заглушка
            MessageBox.Show("Функция 'Шаг вперед' пока в разработке.");
        }

        // Обработчик кнопки "Шаг назад"
        private void StepBackButton_Click(object sender, RoutedEventArgs e)
        {
            // Временная заглушка
            MessageBox.Show("Функция 'Шаг назад' пока в разработке.");
        }
        
        // Отрисовка выбранного фрактала
        private void GenerateFractalButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(RecursionDepth.Text, out int depth) && depth >= 0)
            {
                switch (FractalSelector.SelectedIndex)
                {
                    case 0:
                        _fractal = new SierpinskiCarpet(DisplayCanvas);
                        break;
                    case 1:
                        _fractal = new LeviCurve(DisplayCanvas);
                        break;
                    default:
                        MessageBox.Show("Выберите фрактал.");
                        return;
                }

                if (DisplayCanvas != null)
                {
                    DisplayCanvas.Children.Clear();
                    _fractal.DrawFractal(depth);
                }
            }
            else
            {
                MessageBox.Show("Введите корректную глубину рекурсии.");
            }
        }

        // Отрисовка стержней для Ханойских башен
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
                    Width = _rodWidth * 5,
                    Height = _rodWidth * 1.5,
                    Fill = Brushes.Black
                };
                Canvas.SetLeft(baseRod, canvasCenter + i * rodSpacing - (_rodWidth * 5) / 2);
                Canvas.SetTop(baseRod, DisplayCanvas.Height - 20);
                DisplayCanvas.Children.Add(baseRod);

                _rodPositions[i + 1] = canvasCenter + i * rodSpacing;
            }
        }

        // Отрисовка дисков на первом стержне
        private void DrawDisks(int diskCount)
        {
            if (DisplayCanvas == null) return;

            double rodX = _rodPositions[0];
            double baseY = DisplayCanvas.Height - 20;
            double maxDiskWidth = 130;
            double minDiskWidth = 50;
            double diskWidthStep = (maxDiskWidth - minDiskWidth) / (diskCount - 1);

            for (int i = 0; i < diskCount; i++)
            {
                double diskWidth = maxDiskWidth - i * diskWidthStep;

                var disk = new Ellipse
                {
                    Width = diskWidth,
                    Height = _diskHeight,
                    Fill = Brushes.Black
                };

                Canvas.SetLeft(disk, rodX - diskWidth / 2);
                Canvas.SetTop(disk, baseY - (i + 1) * _diskHeight);

                disk.Tag = new DiskInfo { CurrentRod = 0, PositionOnRod = i };
                DisplayCanvas.Children.Add(disk);
            }
        }

        // Запуск решения задачи Ханойских башен
        private async void StartHanoiButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(DiskCount.Text, out _diskCount) && _diskCount > 0)
            {
                _hanoiTowers = new HanoiTowers();
                _hanoiTowers.Solve(_diskCount, "A", "B", "C");
                await DisplayMoves();
            }
            else
            {
                MessageBox.Show("Введите корректное количество дисков.");
            }
        }

        // Отображение движения дисков
        private async Task DisplayMoves()
        {
            foreach (var move in _hanoiTowers.GetMoves())
            {
                var parsedMove = ParseMove(move);
                if (parsedMove.HasValue)
                {
                    await MoveDisk(parsedMove.Value.Disk, parsedMove.Value.FromRod, parsedMove.Value.ToRod);
                    await Task.Delay(500);
                }
            }
        }

        // Перемещение диска
        private async Task MoveDisk(int diskNumber, int fromRod, int toRod)
        {
            foreach (UIElement element in DisplayCanvas.Children)
            {
                if (element is Ellipse disk && disk.Tag is DiskInfo info && info.CurrentRod == fromRod && info.PositionOnRod == diskNumber)
                {
                    Canvas.SetTop(disk, Canvas.GetTop(disk) - 50);
                    await Task.Delay(200);

                    Canvas.SetLeft(disk, _rodPositions[toRod] - disk.Width / 2);
                    await Task.Delay(200);

                    int newPosition = GetNextAvailablePosition(toRod);
                    Canvas.SetTop(disk, DisplayCanvas.Height - _rodHeight - 20 - newPosition * _diskHeight);
                    info.CurrentRod = toRod;
                    info.PositionOnRod = newPosition;
                    break;
                }
            }
        }

        // Определение следующей доступной позиции на стержне
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

        // Парсинг хода перемещения
        private (int Disk, int FromRod, int ToRod)? ParseMove(string move)
        {
            string[] parts = move.Split(' ');
            if (parts.Length == 5 && int.TryParse(parts[1], out int disk))
            {
                int fromRod = parts[3] == "A" ? 0 : (parts[3] == "B" ? 1 : 2);
                int toRod = parts[4] == "A" ? 0 : (parts[4] == "B" ? 1 : 2);
                return (disk - 1, fromRod, toRod);
            }
            return null;
        }

        private class DiskInfo
        {
            public int CurrentRod { get; set; }
            public int PositionOnRod { get; set; }
        }
    }
}