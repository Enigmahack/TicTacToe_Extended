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

        /// Finds a random available move.

        private (int r, int c) MoveEasy(GameState gameState)
        {
            Random rnd = Random.Shared;

            List<(int r, int c)> availableMoves = GetAvailableMoves(gameState).ToList();
            if (availableMoves.Count > 0)
            {
                int index = rnd.Next(availableMoves.Count);
                return availableMoves[index];
            }

            return (-1, -1);
        }

        /// Implements a strategic move: 1. Win, 2. Block, 3. Center, 4. Corner, 5. Random.

        private (int, int) MoveMedium(GameState gameState)
        {
            Player computer = gameState.ComputerPlayer;
            Player human = gameState.GetOpponent(computer);

            // 1. Check for immediate winning move (Computer moves first)
            foreach ((int r, int c) move in GetAvailableMoves(gameState))
            {
                gameState.MakeTemporaryMove(move.r, move.c);
                GameResult result = gameState.CheckForWin();
                gameState.UndoTemporaryMove(move.r, move.c);

                if (result != null && result.Winner == computer)
                {
                    return move; // Win immediately!
                }
            }

            // 2. Check for immediate blocking move (Human's next move)
            // Temporarily swap player to check if the human has a winning move
            gameState.SwitchPlayer(); // CurrentPlayer is now Human

            (int r, int c) blockMove = (-1, -1);

            foreach ((int r, int c) move in GetAvailableMoves(gameState))
            {
                gameState.MakeTemporaryMove(move.r, move.c); // Places Human piece
                GameResult result = gameState.CheckForWin();
                gameState.UndoTemporaryMove(move.r, move.c); // Restores grid and CurrentPlayer to Human

                if (result != null && result.Winner == human)
                {
                    blockMove = move; // Found the human's winning move.
                    break;
                }
            }

            // Restore the player back to the computer's turn
            gameState.SwitchPlayer(); // CurrentPlayer is now Computer

            if (blockMove.r != -1)
            {
                return blockMove; // Block the human win!
            }

            // 3. Fallback to strategic moves (Center/Corners)

            // Take center (1, 1)
            if (gameState.CanMakeMove(1, 1))
            {
                return (1, 1);
            }

            // Try corners (0,0), (0,2), (2,0), (2,2)
            (int r, int c)[] corners = { (0, 0), (0, 2), (2, 0), (2, 2) };
            foreach ((int r, int c) move in corners)
            {
                if (gameState.CanMakeMove(move.r, move.c))
                {
                    return move;
                }
            }

            // 4. Fallback to random move (MoveEasy handles the rest of the available squares)
            return MoveEasy(gameState);
        }

        /// Implements the Minimax algorithm for an unbeatable AI.

        private (int, int) MoveHard(GameState gameState)
        {
            int bestScore = int.MinValue;
            (int r, int c) bestMove = (-1, -1);
            Player maximizingPlayer = gameState.ComputerPlayer;

            // Optional: Start with a random move to vary the opening, 
            // but for true Minimax, checking all is required.

            foreach ((int r, int c) move in GetAvailableMoves(gameState))
            {
                gameState.MakeTemporaryMove(move.r, move.c);
                // The opponent is the next player, so we call Minimax assuming they will minimize the score.
                int score = Minimax(gameState, 0, maximizingPlayer);
                gameState.UndoTemporaryMove(move.r, move.c);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }

            // If all scores are equal (e.g., all 0 in a tie scenario), pick a random move among them
            if (bestMove.r == -1)
            {
                return GetAvailableMoves(gameState).FirstOrDefault();
            }

            return bestMove;
        }

        /// Minimax implementation to determine the best score for the current state.
        /// <returns>Score: +10 for computer win, -10 for human win, 0 for tie, adjusted by depth.</returns>
        private int Minimax(GameState gameState, int depth, Player maximizingPlayer)
        {
            // Check for terminal state
            GameResult result = gameState.CheckForWin();
            if (result != null)
            {
                if (result.Winner == maximizingPlayer)
                    return 10 - depth; // Win: prioritize faster win
                if (result.Winner == gameState.GetOpponent(maximizingPlayer))
                    return depth - 10; // Loss: prioritize later loss
                return 0; // Tie
            }

            Player currentPlayer = gameState.CurrentPlayer;

            if (currentPlayer == maximizingPlayer) // Maximizer's turn (Computer)
            {
                int bestScore = int.MinValue;
                foreach ((int r, int c) move in GetAvailableMoves(gameState))
                {
                    gameState.MakeTemporaryMove(move.r, move.c);
                    int score = Minimax(gameState, depth + 1, maximizingPlayer);
                    gameState.UndoTemporaryMove(move.r, move.c);
                    bestScore = Math.Max(bestScore, score);
                }
                return bestScore;
            }
            else // Minimizer's turn (Human)
            {
                int bestScore = int.MaxValue;
                foreach ((int r, int c) move in GetAvailableMoves(gameState))
                {
                    gameState.MakeTemporaryMove(move.r, move.c);
                    int score = Minimax(gameState, depth + 1, maximizingPlayer);
                    gameState.UndoTemporaryMove(move.r, move.c);
                    bestScore = Math.Min(bestScore, score);
                }
                return bestScore;
            }
        }

        /// Gets all empty (r, c) coordinates on the board.

        private IEnumerable<(int r, int c)> GetAvailableMoves(GameState gameState)
        {
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    if (gameState.GameGrid[r, c] == Player.None)
                    {
                        yield return (r, c);
                    }
                }
            }
        }
    }
}
