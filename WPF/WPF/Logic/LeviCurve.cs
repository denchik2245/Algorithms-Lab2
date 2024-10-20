﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Logic
{
    public class LeviCurve : IFractal
    {
        private readonly Canvas _canvas;

        public LeviCurve(Canvas canvas)
        {
            _canvas = canvas;
        }

        public void DrawFractal(int depth)
        {
            _canvas.Children.Clear();

            // Применяем глобальное масштабирование к Canvas
            double scalingFactor = 0.75; // Масштабируем фрактал до 75% от исходного размера
            ScaleTransform scale = new ScaleTransform(scalingFactor, scalingFactor);
            _canvas.RenderTransform = scale;

            // Получаем ширину и высоту Canvas
            double canvasWidth = _canvas.ActualWidth;
            double canvasHeight = _canvas.ActualHeight;

            // Вычисляем начальные точки для центрирования фрактала по горизонтали и вертикали
            double startX = (canvasWidth / 4) / scalingFactor;  // Центрирование по горизонтали с учетом масштаба
            double endX = (3 * canvasWidth / 4) / scalingFactor;
            double startY = (canvasHeight / 1.7) / scalingFactor; // Центрирование по вертикали с учетом масштаба

            // Корректируем начальные точки, чтобы фрактал находился по центру вертикали
            startY += (canvasHeight / 4) / scalingFactor;

            Point startPoint = new Point(startX, startY);
            Point endPoint = new Point(endX, startY);

            DrawLevi(depth, startPoint, endPoint);
        }

        private void DrawLevi(int depth, Point startPoint, Point endPoint)
        {
            if (depth == 0)
            {
                // Рисуем линию от начальной до конечной точки
                var line = new Line
                {
                    X1 = startPoint.X,
                    Y1 = startPoint.Y,
                    X2 = endPoint.X,
                    Y2 = endPoint.Y,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                _canvas.Children.Add(line);
            }
            else
            {
                // Находим середину отрезка и смещаем вверх для создания изгиба
                var middleX = (startPoint.X + endPoint.X) / 2;
                var middleY = (startPoint.Y + endPoint.Y) / 2;
                var middlePoint = new Point(middleX + (endPoint.Y - startPoint.Y) / 2, middleY - (endPoint.X - startPoint.X) / 2);

                // Рекурсивно рисуем две линии
                DrawLevi(depth - 1, startPoint, middlePoint);
                DrawLevi(depth - 1, middlePoint, endPoint);
            }
        }
    }
}