using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TicTacToe
{

    // TODO: 
    // Minor bugs with multi-player (2 player, not vs cpu) and re-testing start states


    public partial class MainWindow : Window
    {
        internal static double FadedOpacity = 0.1;
        internal static int UnfadedOpacity = 1;
        internal static double EndScreenOverlayOpacity = 0.6;



        private readonly Dictionary<Player, ImageSource> imageSources = new()
        {
            { Player.X, new BitmapImage(new Uri("pack://application:,,,/Assets/X15.png")) },
            { Player.O, new BitmapImage(new Uri("pack://application:,,,/Assets/O15.png")) }
        };

        private readonly Dictionary<Player, ObjectAnimationUsingKeyFrames> animations = new()
        {
            {Player.X, new ObjectAnimationUsingKeyFrames() },
            {Player.O, new ObjectAnimationUsingKeyFrames() }
        };

        private readonly DoubleAnimation fadeOutAnimation = new DoubleAnimation()
        {
            Duration = TimeSpan.FromSeconds(.5),
            From = UnfadedOpacity,
            To = FadedOpacity,
            FillBehavior = FillBehavior.HoldEnd
        };

        private readonly DoubleAnimation fadeInAnimation = new DoubleAnimation()
        {
            Duration = TimeSpan.FromSeconds(.5),
            From = FadedOpacity,
            To = EndScreenOverlayOpacity,
            FillBehavior = FillBehavior.HoldEnd
        };

        private readonly Image[,] imageControls = new Image[3, 3];
        private readonly GameState gameState = new GameState();
        

        public MainWindow()
        {
            InitializeComponent();
            SetupGameGrid();
            SetupAnimations();

            gameState.MoveMade += OnMoveMade;
            gameState.GameEnded += OnGameEnded;
            gameState.GameRestarted += OnGameRestarted;

        }

        private void SetupGameGrid()
        {
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    Image imageControl = new Image();
                    GameGrid.Children.Add(imageControl);
                    imageControls[r, c] = imageControl;
                }
            }
        }

        private void StartGame()
        {
            gameState.StartGame();
        }

        private void SetupAnimations()
        {
            animations[Player.X].Duration = TimeSpan.FromSeconds(.25);
            animations[Player.O].Duration = TimeSpan.FromSeconds(.25);

            for (int i = 0;i < 16; i++)
            {
                Uri xUri = new Uri($"pack://application:,,,/Assets/X{i}.png");
                BitmapImage xImg = new BitmapImage(xUri);
                DiscreteObjectKeyFrame xKeyFrame = new DiscreteObjectKeyFrame(xImg);
                animations[Player.X].KeyFrames.Add(xKeyFrame);

                Uri oUri = new Uri($"pack://application:,,,/Assets/O{i}.png");
                BitmapImage oImg = new BitmapImage(oUri);
                DiscreteObjectKeyFrame oKeyFrame = new DiscreteObjectKeyFrame(oImg);
                animations[Player.O].KeyFrames.Add(oKeyFrame);
            }
        }

        private async Task FadeOut(UIElement uiElement, double uiElementOpacity)
        {
            uiElement.BeginAnimation(OpacityProperty, fadeOutAnimation);
            await Task.Delay(fadeOutAnimation.Duration.TimeSpan);

            uiElement.Opacity = uiElementOpacity;
            uiElement.BeginAnimation(OpacityProperty, null);
        }

        private async Task FadeIn(UIElement uiElement, double uiElementOpacity)
        {
            uiElement.Opacity = FadedOpacity;
            uiElement.Visibility = Visibility.Visible;

            uiElement.BeginAnimation(OpacityProperty, fadeInAnimation);
            await Task.Delay(fadeInAnimation.Duration.TimeSpan);

            uiElement.Opacity = uiElementOpacity;
            uiElement.BeginAnimation(OpacityProperty, null);
        }

        private async Task TransitionToEndScreen(string text, ImageSource winnerImage)
        {

            await Task.WhenAll(FadeOut(TurnPanel, FadedOpacity), FadeOut(GameCanvas, FadedOpacity));
            ResultText.Text = text;
            WinnerImage.Source = winnerImage;

            EndScreen.Opacity = FadedOpacity;
            await FadeIn(EndScreen, EndScreenOverlayOpacity);
            EndScreen.Visibility = Visibility.Visible;
        }

        private async Task TransitionToGameScreen()
        {
            Line.Visibility = Visibility.Hidden;
            TurnPanel.Visibility = Visibility.Visible;
            await FadeOut(EndScreen, FadedOpacity);
            EndScreen.Visibility = Visibility.Hidden;
            await Task.WhenAll(FadeIn(TurnPanel, UnfadedOpacity), FadeIn(GameCanvas, UnfadedOpacity));
        }

        private (Point, Point) FindLinePoints(WinInfo winInfo)
        {
            double squareSize = GameGrid.Width / 3;
            double margin = squareSize / 2;

            if (winInfo.Type == WinType.Row)
            {
                double y = winInfo.Number * squareSize + margin; 
                return (new Point(0, y), new Point(GameGrid.Width, y));
            }
            if (winInfo.Type == WinType.Column)
            {
                double x = winInfo.Number * squareSize + margin;
                return (new Point(x, 0), new Point(x, GameGrid.Height));
            }
            if (winInfo.Type == WinType.LRDiag)
            {
                return (new Point(0, 0), new Point(GameGrid.Width, GameGrid.Height));
            }
            return (new Point(GameGrid.Width, 0), new Point(0, GameGrid.Height));
        }

        private async Task ShowLine(WinInfo winInfo)
        {
            (Point start, Point end) = FindLinePoints(winInfo);

            Line.X1 = start.X;
            Line.Y1 = start.Y;

            DoubleAnimation x2Animation = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(.25),
                From = start.X,
                To = end.X
            };

            DoubleAnimation y2Animation = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(.25),
                From = start.Y,
                To = end.Y
            };

            Line.Visibility = Visibility.Visible;
            Line.BeginAnimation(Line.X2Property, x2Animation);
            Line.BeginAnimation(Line.Y2Property, y2Animation);
            await Task.Delay(x2Animation.Duration.TimeSpan);
        }

        private void OnMoveMade(int r, int c)
        {
            Player player = gameState.GameGrid[r,c];
            imageControls[r, c].BeginAnimation(Image.SourceProperty, animations[player]);
            PlayerImage.Source = imageSources[gameState.CurrentPlayer];
        }

        private async void OnGameEnded(GameResult gameResult)
        {
            await Task.Delay(1000);

            if(gameResult.Winner == Player.None)
            {
                await TransitionToEndScreen("It's a tie!", null);
            } 
            else
            {
                await ShowLine(gameResult.WinInfo);
                await Task.Delay(1000);
                await TransitionToEndScreen("Winner:", imageSources[gameResult.Winner]);
            }
        }

        private async void OnGameRestarted()
        {
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    imageControls[r, c].BeginAnimation(Image.SourceProperty, null);
                    imageControls[r, c].Source = null;
                }
            }

            PlayerImage.Source = imageSources[gameState.CurrentPlayer];
            await TransitionToGameScreen();
        }

        private void GameGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            double squareSize = GameGrid.Width / 3;
            Point clickPosition = e.GetPosition(GameGrid);
            int row = (int)(clickPosition.Y / squareSize);
            int col = (int)(clickPosition.X / squareSize);
            gameState.MakeMove(row, col);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            Button myButton = (Button)sender;
            string buttonText = myButton.Tag.ToString();

            switch (buttonText)
            {
                case "PlayAgain":
                    if (gameState.GameOver)
                    {
                        gameState.Reset();
                    }
                    break;
                case "Settings":
                    ShowSettings();
                    break;

                case "Quit":
                    ShutDown();
                    break;

                default:
                    break;
            }
        }

        private void Button_Click_Start(object sender, RoutedEventArgs e)
        {
            WelcomeScreen.Visibility = Visibility.Hidden;
            StartGame();
        }

        private async void Button_Click_Settings(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            button.IsEnabled = false;

            try
            {
                // 1. Primary Action (e.g., Save, Start Game, etc.)
                await ShowSettings(); // Use async/await for I/O or long tasks

            }
            finally
            {
                // Re-enable the button once the action is finished, even if an error occurred
                button.IsEnabled = true;
            }
        }

        private void Button_Click_Quit(object sender, RoutedEventArgs e)
        {
            ShutDown();
        }

        private Task ShowSettings()
        {
            TicTacToe.Settings settingsWindow = new(this.gameState);
            settingsWindow.ShowDialog();
            return Task.CompletedTask;
        }

        private void ShutDown()
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}