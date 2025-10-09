using System;
using System.ComponentModel;

namespace TicTacToe
{
    internal class GameState : INotifyPropertyChanged
    {

        private Difficulty _aiDifficulty = Difficulty.Easy;
        private Player _humanPlayer = Player.None;
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
        public Player HumanPlayer 
        {
            get => _humanPlayer;
            set
            {
                if (_humanPlayer != value)
                {
                    _humanPlayer = value;
                    OnPropertyChanged(nameof(HumanPlayer));
                }
            }
        } 
        public bool AIEnabled { get; private set; }
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

        internal bool CanMakeMove(int r, int c)
        {
            return !GameOver && GameGrid[r, c] == Player.None;
        }

        private bool IsGridFull()
        {
            return TurnsPassed == 9;
        }

        private void SwitchPlayer()
        {
            CurrentPlayer = CurrentPlayer == Player.O ? Player.X : Player.O;
            // Works like this: 
            // Current player = if (Current Player is Player.O), then Player.X, else Player.O
            // It works this way because there is only a single statement within each section. 
        }

        public void ComputerPlayerActive(bool enable)
        {
            AIEnabled = enable; 
        }

        public void SetComputerPlayer(Player player)
        {
            ComputerPlayer = player;
        }

        public void SetHumanPlayer(Player player)
        {
            HumanPlayer = player;
        }

        public void SetPlayerRandomly()
        {
            HumanPlayer = Player.Random;
        }

        private void PlayerRandomize()
        {
            Player player = Random.Shared.Next(2) == 0 ? Player.X : Player.O;
            SetHumanPlayer(player);
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
            ComputerPlayer = Player.None;
            TurnsPassed = 0;
            GameOver = false;
            GameRestarted?.Invoke();
        }
    }
}
