using System;
using System.Collections.Generic;

namespace Logic
{
    public class HanoiTowers
    {
        public List<string> Moves { get; private set; }

        public HanoiTowers()
        {
            Moves = new List<string>();
        }

        // Метод для начала решения задачи
        public void Solve(int n, string source, string auxiliary, string destination)
        {
            Moves.Clear();
            MoveDisks(n, source, auxiliary, destination);
        }

        // Рекурсивный метод для перемещения дисков
        private void MoveDisks(int n, string source, string auxiliary, string destination)
        {
            if (n == 1)
            {
                // Базовый случай - переместить один диск
                Moves.Add($"Переместить диск 1 с {source} на {destination}");
            }
            else
            {
                // Перемещаем n-1 диск с source на auxiliary
                MoveDisks(n - 1, source, destination, auxiliary);
                // Перемещаем последний диск с source на destination
                Moves.Add($"Переместить диск {n} с {source} на {destination}");
                // Перемещаем n-1 диск с auxiliary на destination
                MoveDisks(n - 1, auxiliary, source, destination);
            }
        }

        // Метод для получения списка шагов
        public List<string> GetMoves()
        {
            return Moves;
        }
    }
}