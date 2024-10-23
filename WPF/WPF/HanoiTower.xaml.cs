using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;

namespace WPF
{
    public partial class HanoiTower : Window
    {
        private Stack<int>[] h;  // Башни, хранящие диски
        private Dictionary<int, Rectangle> diskVisuals; // Словарь для хранения визуальных элементов дисков

        public HanoiTower()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            // Читаем количество дисков из текстового поля
            if (!int.TryParse(diskCountTextBox.Text, out int diskCount) || diskCount < 1)
            {
                MessageBox.Show("Please enter a valid number of disks (positive integer).", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Инициализируем башни
            h = new Stack<int>[3];
            h[0] = new Stack<int>();
            h[1] = new Stack<int>();
            h[2] = new Stack<int>();

            diskVisuals = new Dictionary<int, Rectangle>();

            // Очищаем холст перед созданием новых дисков
            canvas.Children.Clear();

            // Создаем диски и добавляем их на первую башню
            for (int i = diskCount; i >= 1; i--)
            {
                h[0].Push(i);
                CreateDisk(i);
                await Task.Delay(100); // Небольшая задержка для визуализации добавления дисков
            }

            // Запускаем алгоритм решения
            await Hanoi(1, 3, diskCount);
        }

        private async Task Hanoi(int from, int to, int diskCount)
        {
            int intermediate = 6 - from - to; // Промежуточная башня

            if (diskCount > 1)
            {
                await Hanoi(from, intermediate, diskCount - 1);
                await Hanoi(from, to, 1);
                await Hanoi(intermediate, to, diskCount - 1);
            }
            else
            {
                // Проверка на пустой стек
                if (h[from - 1].Count > 0)
                {
                    int diskSize = h[from - 1].Pop();
                    h[to - 1].Push(diskSize);
                    await MoveDiskVisual(diskSize, to);
                }
                else
                {
                    MessageBox.Show($"Stack {from} is empty. Cannot move disk.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Создаем визуальный элемент для диска
        private void CreateDisk(int size)
        {
            Rectangle rect = new Rectangle
            {
                Width = size * 20,
                Height = 20,
                Fill = Brushes.Blue,
                Stroke = Brushes.Black
            };

            canvas.Children.Add(rect);
            diskVisuals[size] = rect;
            UpdateDiskPosition(size, 1);
        }

        // Обновляем позицию диска на башне
        private void UpdateDiskPosition(int disk, int tower)
        {
            if (diskVisuals.TryGetValue(disk, out var rect))
            {
                int index = h[tower - 1].Count - 1;
                double x = (tower - 1) * 250 + 50 - rect.Width / 2;
                double y = canvas.ActualHeight - (index + 1) * 22;

                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y);
            }
        }

        // Анимация перемещения диска между башнями
        private async Task MoveDiskVisual(int diskSize, int toTower)
        {
            if (diskVisuals.TryGetValue(diskSize, out var rect))
            {
                // Начальные координаты
                double startX = Canvas.GetLeft(rect);
                double startY = Canvas.GetTop(rect);

                // Целевые координаты
                double targetX = (toTower - 1) * 250 + 50 - rect.Width / 2;
                double targetY = canvas.ActualHeight - h[toTower - 1].Count * 22;

                // Анимация перемещения
                int steps = 30;
                for (int i = 0; i <= steps; i++)
                {
                    double newX = startX + (targetX - startX) * i / steps;
                    double newY = startY - 10 * Math.Sin(Math.PI * i / steps) + (targetY - startY) * i / steps;
                    Canvas.SetLeft(rect, newX);
                    Canvas.SetTop(rect, newY);
                    await Task.Delay(10); // Задержка между шагами
                }
            }
            else
            {
                MessageBox.Show($"Rectangle for disk {diskSize} not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
