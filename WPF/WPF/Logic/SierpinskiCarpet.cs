using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Logic
{
    public class SierpinskiCarpet : IFractal
    {
        private readonly Canvas _canvas;

        public SierpinskiCarpet(Canvas canvas)
        {
            _canvas = canvas;
        }

        public void DrawFractal(int depth)
        {
            _canvas.Children.Clear();
            DrawCarpet(depth, 0, 0, _canvas.Width, _canvas.Height);
        }

        private void DrawCarpet(int depth, double x, double y, double width, double height)
        {
            if (depth == 0)
            {
                // Рисуем прямоугольник
                var rect = new Rectangle
                {
                    Width = width,
                    Height = height,
                    Fill = Brushes.Black
                };
                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y);
                _canvas.Children.Add(rect);
            }
            else
            {
                // Разделяем на 9 частей, центральную часть оставляем пустой
                double newWidth = width / 3;
                double newHeight = height / 3;

                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (i == 1 && j == 1)
                        {
                            // Центральный квадрат
                            continue;
                        }
                        DrawCarpet(depth - 1, x + i * newWidth, y + j * newHeight, newWidth, newHeight);
                    }
                }
            }
        }
    }
}