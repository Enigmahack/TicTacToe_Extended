using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace TicTacToe
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        private readonly GameState gameState;

        internal Settings(GameState gameState)
        {
            InitializeComponent();
            this.DataContext = gameState;
            this.gameState = gameState;
        }

        private void Button_Click_PlayerSelect(object sender, RoutedEventArgs e)
        {
            Button myButton = (Button)sender;
            switch (myButton.Tag.ToString())
            {
                case "X":
                    gameState.SetHumanPlayer(Player.X); 
                    break;
                case "O":
                    gameState.SetHumanPlayer(Player.O); 
                    break;
                case "Random":
                    gameState.SetPlayerRandomly(); 
                    break;
                default:
                    gameState.SetHumanPlayer(Player.X); 
                    break;
            }
        }

        private void Button_Click_DifficultySelect(object sender, RoutedEventArgs e)
        {
            Button myButton = (Button)sender;

            if(Enum.TryParse(myButton.Tag.ToString(), out Difficulty difficulty))
            {
                gameState.AIDifficulty = difficulty;
            }
        }

        private void Button_Click_OK(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void EnableComputer_ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
           bool isEnabled = (sender as ToggleButton).IsChecked == true;

            if (!isEnabled) 
            {
                DifficultySelect_Easy.IsEnabled = false;
                DifficultySelect_Medium.IsEnabled = false;
                DifficultySelect_Hard.IsEnabled = false;
            } 
            else
            {
                DifficultySelect_Easy.IsEnabled = true;
                DifficultySelect_Medium.IsEnabled = true;
                DifficultySelect_Hard.IsEnabled = true;
            }

                gameState.ComputerPlayerActive(isEnabled);
        }
    }
}
