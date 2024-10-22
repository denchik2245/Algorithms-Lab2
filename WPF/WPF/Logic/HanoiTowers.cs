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
        
        public void Solve(int n, string source, string auxiliary, string destination)
        {
            Moves.Clear();
            MoveDisks(n, source, auxiliary, destination);
        }
        
        private void MoveDisks(int n, string source, string auxiliary, string destination)
        {
            if (n == 1)
            {
                Moves.Add($"Переместить диск 1 с {source} на {destination}");
            }
            else
            {
                MoveDisks(n - 1, source, destination, auxiliary);
                Moves.Add($"Переместить диск {n} с {source} на {destination}");
                MoveDisks(n - 1, auxiliary, source, destination);
            }
        }
        
        public List<string> GetMoves()
        {
            return Moves;
        }
    }
}