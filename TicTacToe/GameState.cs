using System;
using System.ComponentModel;

namespace TicTacToe
{
    internal class GameState : INotifyPropertyChanged
    {

        private Difficulty _aiDifficulty = Difficulty.Easy;
        private Player _humanPlayerSelection = Player.None;
        private bool _aiEnabled = false;
        public Player[,] GameGrid { get; private set; }
        public Player CurrentPlayer { get; private set; }
        public Player ComputerPlayer { get; private set; }
        public Difficulty AIDifficulty
        {
            get => _aiDifficulty;
            set
            {
                if (_aiDifficulty != value)
                {
                    _aiDifficulty = value;
                    OnPropertyChanged(nameof(AIDifficulty));
                }
            }
        }
        public Player HumanPlayer { get; private set; }
        public Player HumanPlayerSelection
        {
            get => _humanPlayerSelection;
            set
            {
                if (_humanPlayerSelection != value)
                {
                    _humanPlayerSelection = value;
                    OnPropertyChanged(nameof(HumanPlayerSelection));
                }
            }
        }
        public bool AIEnabled 
        {
            get => _aiEnabled;
            set 
            {
                if (_aiEnabled != value) 
                {
                    _aiEnabled = value;
                    OnPropertyChanged(nameof(AIEnabled));
                }
            }
        }
        public int TurnsPassed { get; private set; }
        public bool GameOver { get; private set; }



        private readonly ComputerMove computerMove = new();

        public event Action<int, int> MoveMade;
        public event Action<GameResult> GameEnded;
        public event Action GameRestarted;
        public event PropertyChangedEventHandler? PropertyChanged;

        public GameState()
        {
            GameGrid = new Player[3, 3];
            CurrentPlayer = Player.X;
            ComputerPlayer = Player.None;
            TurnsPassed = 0;
            GameOver = false;
        }

        public void StartGame()
        {
            if (AIEnabled)
            {
                CheckHumanPlayer();
                SetComputerPlayer();
            } else
            {
                ComputerPlayer = Player.None;
            }

            if (ComputerPlayer == Player.X)
            {
                HandleComputerTurn();
            }
        }

        internal bool CanMakeMove(int r, int c)
        {
            return !GameOver && GameGrid[r, c] == Player.None;
        }

        private bool IsGridFull()
        {
            return TurnsPassed == 9;
        }

        internal void SwitchPlayer()
        {
            CurrentPlayer = CurrentPlayer == Player.O ? Player.X : Player.O; 
        }

        public Player GetOpponent(Player player)
        {
            if (player == Player.X) return Player.O;
            if (player == Player.O) return Player.X;
            return Player.None;
        }

        // --- Minimax Helper Methods ---
        /// Makes a move without firing events or checking for game over. Used for AI simulation.
        public void MakeTemporaryMove(int r, int c)
        {
            GameGrid[r, c] = CurrentPlayer;
            TurnsPassed++;
            SwitchPlayer();
        }


        /// Undoes a simulated move by reverting the grid, turn count, and player.
        public void UndoTemporaryMove(int r, int c)
        {
            GameGrid[r, c] = Player.None;
            TurnsPassed--;
            SwitchPlayer(); // Switches the player back to who made the move
        }

        /// Checks the current grid state for a win or tie, ignoring the internal GameOver flag.
        /// Returns GameResult if the game is over, otherwise null.
        /// 
        public GameResult CheckForWin()
        {
            // Check all 3 rows and 3 columns
            for (int i = 0; i < 3; i++)
            {
                // Check Row i
                if (GameGrid[i, 0] != Player.None && GameGrid[i, 0] == GameGrid[i, 1] && GameGrid[i, 0] == GameGrid[i, 2])
                    return new GameResult { Winner = GameGrid[i, 0], WinInfo = new WinInfo { Type = WinType.Row, Number = i } };

                // Check Column i
                if (GameGrid[0, i] != Player.None && GameGrid[0, i] == GameGrid[1, i] && GameGrid[0, i] == GameGrid[2, i])
                    return new GameResult { Winner = GameGrid[0, i], WinInfo = new WinInfo { Type = WinType.Column, Number = i } };
            }

            // Check main diagonal (top-left to bottom-right)
            if (GameGrid[0, 0] != Player.None && GameGrid[0, 0] == GameGrid[1, 1] && GameGrid[0, 0] == GameGrid[2, 2])
                return new GameResult { Winner = GameGrid[0, 0], WinInfo = new WinInfo { Type = WinType.LRDiag } };

            // Check anti-diagonal (top-right to bottom-left)
            if (GameGrid[0, 2] != Player.None && GameGrid[0, 2] == GameGrid[1, 1] && GameGrid[0, 2] == GameGrid[2, 0])
                return new GameResult { Winner = GameGrid[0, 2], WinInfo = new WinInfo { Type = WinType.RLDiag } };

            // Check for draw
            if (TurnsPassed == 9)
                return new GameResult { Winner = Player.None }; // Tie

            return null; // Game not over
        }


        public void SetComputerPlayer()
        {
            if (HumanPlayer == Player.X)
            {
                ComputerPlayer = Player.O;
            } 
            else
            {
                ComputerPlayer = Player.X;
            }
        }

        public void CheckHumanPlayer()
        {
            if(HumanPlayerSelection == Player.Random)
            {
                SetRandomPlayer();
            }
        }

        public void SetRandomPlayer()
        {
            Random rand = new Random();
            int i = rand.Next(2);
            if (i == 0)
            {
                HumanPlayer = Player.X;
            } else
            {
                HumanPlayer = Player.O;
            }
        }

        public void SetHumanPlayer(Player player)
        {
            HumanPlayerSelection = player;
            HumanPlayer = player;
        }

        private bool AreSquaresMarked((int, int)[] squares, Player player)
        {
            foreach ((int r, int c) in squares)
            {
                if (GameGrid[r, c] != player)
                {
                    return false;
                }
            }
            return true;
        }

        private bool DidMoveWin(int r, int c, out WinInfo winInfo)
        {
            (int, int)[] row = new[] { (r, 0), (r, 1), (r, 2) };
            (int, int)[] col = new[] { (0, c), (1, c), (2, c) };
            (int, int)[] mainDiag = new[] { (0, 0), (1, 1), (2, 2) };
            (int, int)[] antiDiag = new[] { (0, 2), (1, 1), (2, 0) };

            if (AreSquaresMarked(row, CurrentPlayer))
            {
                winInfo = new WinInfo { Type = WinType.Row, Number = r };
                return true;
            }

            if (AreSquaresMarked(col, CurrentPlayer))
            {
                winInfo = new WinInfo { Type = WinType.Column, Number = c };
                return true;
            }

            if (AreSquaresMarked(mainDiag, CurrentPlayer))
            {
                winInfo = new WinInfo { Type = WinType.LRDiag };
                return true;
            }

            if (AreSquaresMarked(antiDiag, CurrentPlayer))
            {
                winInfo = new WinInfo { Type = WinType.RLDiag };
                return true;
            }

            winInfo = null;
            return false;
        }

        private bool DidMoveEndGame(int r, int c, out GameResult gameResult) 
        {
            if(DidMoveWin(r,c, out WinInfo winInfo))
            {
                gameResult = new GameResult { Winner = CurrentPlayer, WinInfo = winInfo };
                return true;
            }

            if (IsGridFull())
            {
                gameResult = new GameResult { Winner = Player.None };
                return true;
            }

            gameResult = null;
            return false;
        }


        public void MakeMove(int r, int c)
        {
            if (!CanMakeMove(r, c))
            {
                return; 
            }

            GameGrid[r, c] = CurrentPlayer;
            TurnsPassed++;

            if (DidMoveEndGame(r,c,out GameResult gameResult))
            {
                GameOver = true;
                MoveMade?.Invoke(r, c);
                GameEnded?.Invoke(gameResult);
            } else
            {
                SwitchPlayer();
                MoveMade?.Invoke(r, c);
                HandleComputerTurn();
            }
        } 

        public void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void HandleComputerTurn()
        {
            if (CurrentPlayer == ComputerPlayer)
            {
                (int r, int c) = computerMove.ComputerMakeMove(AIDifficulty, this);

                MakeMove(r, c);
            }
        }

        public void Reset()
        {
            GameGrid = new Player[3, 3];
            CurrentPlayer = Player.X;
            TurnsPassed = 0;
            GameOver = false;
            CheckHumanPlayer();
            GameRestarted?.Invoke();
            StartGame();
        }
    }
}
