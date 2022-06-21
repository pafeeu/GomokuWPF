using GomokuWPF.API;
using GomokuWPF.API.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GomokuWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MyApi api;
        private PlayerResponse? selectedPlayer;
        private GameController? gameController;
        private long? gameId;
        private string view;
        public MainWindow()
        {
            InitializeComponent();
            api = new MyApi();
            GoTo("login");
            //api.Login("string2", "Password123$");
        }

        public void Log(string s)
        {
            log.Text = s + "\n" + log.Text;
            //(log.Parent as ScrollViewer).ScrollToBottom();
        }
        private async void GoTo(string view)
        {
            WaitCursor();
            this.view = view;
            gameController = null;
            LoginPanel.Visibility = Visibility.Hidden;
            HomePanel.Visibility = Visibility.Hidden;
            GamePanel.Visibility = Visibility.Hidden;

            MenuBack.Visibility = Visibility.Collapsed;
            MenuCloseGame.Visibility = Visibility.Collapsed;
            MenuLogout.Visibility = Visibility.Collapsed;


            switch (view)
            {
                case "login":
                    LoginPanel.Visibility = Visibility.Visible;
                    if (!string.IsNullOrEmpty(log.Text))
                        Log("WYLOGOWANO");
                    api.Logout();
                    break;
                case "home":
                    HomePanel.Visibility = Visibility.Visible;
                    MenuLogout.Visibility = Visibility.Visible;
                    AskForNewGames();
                    break;
                case "game":
                    GamePanel.Visibility = Visibility.Visible;
                    gameController = new GameController((long)gameId, api, Board, this);
                    MenuLogout.Visibility = Visibility.Visible;
                    MenuCloseGame.Visibility = Visibility.Visible;
                    MenuBack.Visibility = Visibility.Visible;
                    //tempGameInfo.Text = string.Join("\n", await api.GetGameDetails((long)gameId));
                    break;

                default:
                    Log("WYSTĄPIŁ BŁĄD, WYLOGOWANO");
                    GoTo("login");
                    break;
            }
            selectedPlayer = null;
            gameId = null;
            ResetCursor();
        }
        private async void AskForNewGames()
        {
            while (view == "home")
            {
                if (selectedPlayer is not null)
                    AllGames.ItemsSource = await api.GetGames(selectedPlayer.Id);
                else
                    AllGames.ItemsSource = await api.GetGames();

                MyGames.ItemsSource = await api.GetGames(api.AboutMe.PlayerId);

                if (string.IsNullOrEmpty(search_player.Text))
                    PlayersList.ItemsSource = await api.GetPlayers();
                else
                    PlayersList.ItemsSource = await api.GetPlayers(search_player.Text);

                await Task.Delay(1000);
            }
        }

        //login and register in login panel
        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            WaitCursor();
            if (await api.Login(llogin.Text, lpassword.Password)) {
                Log($"ZALOGOWANO ({api.AboutMe.Username})");
                GoTo("home");
            }
            else
                Log(api.Logs.Last());
            ResetCursor();
        }

        private async void Register_Click(object sender, RoutedEventArgs e)
        {
            WaitCursor();
            if (await api.Register(rlogin.Text, rpassword.Password, remail.Text))
                Log("ZAREJESTROWANO");
            else
                Log(api.Logs.Last());
            ResetCursor();
        }
        
        //join or create game in home panel
        private async void AllGames_DoubleClick(object sender, RoutedEventArgs e)
        {
            WaitCursor();
            if (AllGames.SelectedIndex < 0)
                return;

            var gameItem = AllGames.SelectedItem as GameResponse;
            if (gameItem is null)
                return;

            gameId = gameItem.Id;

            var game = await api.GetGameDetails((long)gameId);
            if (game is null)
            {
                Log(api.Logs.Last());
                gameId = null;
                return;
            }

            if (game.Players.Any(p => p.PlayerId == api.AboutMe.PlayerId))
            {
                if (game.StatusId == 1)
                    await api.JoinToTheGame((long)gameId);
                Log("POŁĄCZONO DO GRY " + gameId);
                GoTo("game");
            }
            else if(game.Players.Count() < 2)
            {
                if (await api.JoinToTheGame((long)gameId))
                {
                    Log("DOŁĄCZONO DO GRY " + gameId);
                    GoTo("game");
                }
                else
                {
                    Log(api.Logs.Last());
                    gameId = null;
                }
            }
            else
            {
                Log("NIE MOŻESZ DOŁĄCZYĆ DO GRY "+gameId);
                gameId = null;
            }

            ResetCursor();
        }
        private async void MyGames_DoubleClick(object sender, RoutedEventArgs e)
        {
            WaitCursor();
            //just connect to the game
            if (MyGames.SelectedIndex >= 0)
            {
                var game = MyGames.SelectedItem as GameResponse;
                if (game is not null)
                {
                    gameId = game.Id;
                    if (game.StatusId == 1)
                        await api.JoinToTheGame((long)gameId);
                    Log("POŁĄCZONO DO GRY " + gameId);
                    GoTo("game");
                }
            }
            ResetCursor();
        }
        private void PlayersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(PlayersList.SelectedIndex >= 0)
            {
                selectedPlayer = PlayersList.SelectedItem as PlayerResponse;
            }
        }
        private async void CreateGame_Click(object sender, RoutedEventArgs e)
        {
            WaitCursor();
            gameId = await api.CreateGame(selectedPlayer?.Id);
            if (gameId != null)
            {
                Log("UTWORZONO GRE " + gameId);
                GoTo("game");
            }
            else
                Log(api.Logs.Last());
            ResetCursor();
        }

        //menu items click
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            GoTo("home");
        }
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            api.Logout();
            GoTo("login");
        }
        private void Info_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Projekt gry Gomoku w postaci programu WPF oraz WebAPI został utworzony na zaliczenie projektu z przedmiotu Szkolenie Techniczne 1\nAutor: Paweł Maziarz","Informacje");
        }
        private async void CloseGame_Click(object sender, RoutedEventArgs e)
        {
            if(gameController?.GameInfo.Id != null && view=="game")
            {
                if(gameController?.GameInfo.StatusId == 10)
                {
                    GoTo("home");
                    return;
                }

                if (await api.CloseTheGame(gameController.GameInfo.Id))
                {
                    GoTo("home");
                    Log("ZAMKNIĘTO GRĘ");
                }
                else
                {
                    Log(api.Logs.Last());
                }
            }
        }

        public static void WaitCursor() =>
            Mouse.OverrideCursor = Cursors.Wait;
        public static void ResetCursor() =>
            Mouse.OverrideCursor = null;
    }
}
