using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe
{
    internal class ComputerMove
    {

        private delegate (int r, int c) MoveStrategy(GameState gameState);

        public (int r, int c) ComputerMakeMove(Difficulty difficulty, GameState gameState)
        {
            MoveStrategy moveMethod = null;

            switch (difficulty)
            {
                case Difficulty.Easy:
                    moveMethod = MoveEasy;
                    break;
                case Difficulty.Medium:
                    moveMethod = MoveMedium;
                    break;
                case Difficulty.Hard:
                    moveMethod = MoveHard;
                    break;
                default:
                    throw new ArgumentException("Invalid difficulty level.");
            }

            return moveMethod(gameState);
        }

        private (int r, int c) MoveEasy(GameState gameState)
        {
            Random rnd = Random.Shared;
            int r, c;

            do
            {
                r = rnd.Next(3);
                c = rnd.Next(3);
            } while (!gameState.CanMakeMove(r, c));

            return (r, c);
        }

        private (int, int) MoveMedium(GameState gameState)
        {
            Random rnd = Random.Shared;
            int r, c;

            do
            {
                r = rnd.Next(3);
                c = rnd.Next(3);
            } while (!gameState.CanMakeMove(r, c));

            return (r, c);
        }
        private (int, int) MoveHard(GameState gameState) 
        {
            Random rnd = Random.Shared;
            int r, c;

            do
            {
                r = rnd.Next(3);
                c = rnd.Next(3);
            } while (!gameState.CanMakeMove(r, c));

            return (r, c);
        }
    }
}
