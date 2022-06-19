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
            log.Text += s + "\n";
        }
        private void GoTo(string view)
        {
            WaitCursor();
            this.view = view;
            LoginPanel.Visibility = Visibility.Hidden;
            HomePanel.Visibility = Visibility.Hidden;

            selectedPlayer = null;
            gameId = null;

            switch (view)
            {
                case "login":
                    LoginPanel.Visibility = Visibility.Visible;
                    api.Logout();
                    break;
                case "home":
                    HomePanel.Visibility = Visibility.Visible;
                    AskForNewGames();
                    break;
                default: break;
            }
            ResetCursor();
        }
        private async void AskForNewGames()
        {
            while(true)
            {
                if(view == "home")
                {
                    AllGames.ItemsSource = await api.GetGames();
                    MyGames.ItemsSource = await api.GetGames(api.AboutMe.PlayerId);
                    if(string.IsNullOrEmpty(search_player.Text))
                        PlayersList.ItemsSource = await api.GetPlayers();
                    else
                        PlayersList.ItemsSource = await api.GetPlayers(search_player.Text);

                }
                await Task.Delay(2000);
            }
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            WaitCursor();
            if (await api.Login(llogin.Text, lpassword.Password)) {
                Log("ZALOGOWANO");
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
        

        private void AllGames_DoubleClick(object sender, RoutedEventArgs e)
        {
            WaitCursor();
            //try join (checking im in or not)
            ResetCursor();
        }
        private void MyGames_DoubleClick(object sender, RoutedEventArgs e)
        {
            WaitCursor();
            //just connect to the game
            if (MyGames.SelectedIndex >= 0)
            {
                gameId = MyGames.SelectedIndex;
                GoTo("game");
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
            if(gameId!=null)
                GoTo("game");
            ResetCursor();
        }
        public static void WaitCursor() =>
            Mouse.OverrideCursor = Cursors.Wait;
        public static void ResetCursor() =>
            Mouse.OverrideCursor = null;
    }
}
