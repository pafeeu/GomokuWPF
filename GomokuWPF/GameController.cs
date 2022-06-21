using GomokuWPF.API;
using GomokuWPF.API.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static GomokuWPF.API.Responses.GameDetailsResponse;

namespace GomokuWPF
{
    public class GameController
    {
        private MyApi Api;
        private const short BOARD_SIZE = 19;
        public GameDetailsResponse GameInfo;
        private StackPanel BoardPanel;
        private List<Button> Board;
        private char MySymbol, OtherSymbol;
        private MainWindow Window;

        private bool TemporaryPause = false;
        private bool Redrawing = false;

        public GameController(long gameId, MyApi api, StackPanel board, MainWindow window)
        {
            BoardPanel = board;
            Board = new List<Button>() { };
            Api = api;
            Window = window;
            MySymbol = api.AboutMe.Symbol;
            if(MySymbol == 'x' || MySymbol == 'X')
            {
                MySymbol = 'X';
                OtherSymbol = 'O';
            } 
            else
            {
                MySymbol = 'O';
                OtherSymbol = 'X';
            }
            Init(gameId);
        }
        private async void Init(long gameId)
        {
            MainWindow.WaitCursor();
            var game = await Api.GetGameDetails(gameId);
            if (game is null)
            {
                MessageBox.Show("Nie znaleziono gry");
                return;
            }
            GameInfo = game;

            int FIELD_SIZE = 19;
            int FIELD_MARGIN = 1;
            for (short y = 0; y < BOARD_SIZE; y++)
            {
                var row = new WrapPanel();

                for (short x = 0; x < BOARD_SIZE; x++)
                {
                    var field = new Button();
                    field.Content = "";
                    field.Width = FIELD_SIZE;
                    field.Height = FIELD_SIZE;
                    field.Margin = new Thickness(FIELD_MARGIN, FIELD_MARGIN, FIELD_MARGIN, FIELD_MARGIN);
                    field.CommandParameter = new Coords(x, y);
                    field.Click += (sender, args) => FieldClick(sender);
                    row.Children.Add(field);
                    Board.Add(field);
                }

                BoardPanel.Children.Insert(0, row);
            }


            RedrawBoard();

            MainWindow.ResetCursor();
            AskForNewMoves();
        }
        private void Log(string text)
        {
            Window.Log(text);
        }
        private async void FieldClick(object sender)
        {
            MainWindow.WaitCursor();
            var param = (sender as Button)?.CommandParameter;
            if (GameInfo.StatusId==10 || param is null || param is not Coords)
            {
                MainWindow.ResetCursor();
                return;
            }
            var coords = (Coords)param;
            if (!IsValidMove(coords) || !IsFreeSlot(coords) || WhoseMove() != Api.AboutMe.PlayerId)
            {
                MainWindow.ResetCursor();
                return;
            }
            //MessageBox.Show($"click at {coords.x}, {coords.y}");
            if (!await MakeMove(coords))
            {
                //maybe game is closed
                var newGame = await Api.GetGameDetails(GameInfo.Id);
                if(newGame != null && newGame.StatusId == 10)
                {
                    GameInfo = newGame;
                    RedrawBoard();
                }
                else 
                    Log($"something goes wrong while click at {coords.x}, {coords.y}");

            }
                //MessageBox.Show($"something goes wrong while click at {coords.x}, {coords.y}");

            MainWindow.ResetCursor();
        }
        private void DrawMove(Move move)
        {
            var field = Board.FirstOrDefault(f => ((Coords)f.CommandParameter).x == move.X && ((Coords)f.CommandParameter).y == move.Y);
            if (field != null)
            {
                //field.Content = move.PlayerId;
                if(move.PlayerId == Api.AboutMe.PlayerId && MySymbol == 'X')
                {
                    field.Content = MySymbol;
                    field.Foreground = Brushes.Red;
                } 
                else
                {
                    field.Content = OtherSymbol;
                    field.Foreground = Brushes.Green;
                }
                if (GameInfo.StatusId < 10 && !Redrawing)
                {
                    Log($"Ruch '{GameInfo.Players.First(x=>x.PlayerId==move.PlayerId).Username}' na {move.X}, {move.Y}");
                }
            }
        }
        private void RedrawBoard()
        {
            ClearBoard();
            Redrawing = true;
            foreach(var move in GameInfo.Moves)
            {
                DrawMove(move);
            }
            Redrawing = false;

            if (GameInfo.StatusId == 10)
            {
                SetEnabledBoard(false);
                Window.MenuCloseGame.Visibility = Visibility.Collapsed;
                Log("GRA JEST ZAMKNIĘTA");
            }
            EndIfSomeoneWin();
        }
        private async Task<bool> MakeMove(Coords coords)
        {
            TemporaryPause = true;
            var adding = await Api.MakeMove(GameInfo.Id, coords.x, coords.y);
            if (adding is null)
                return false;

            var move = new Move() { PlayerId=Api.AboutMe.PlayerId, X=coords.x, Y=coords.y };

            GameInfo.Moves.Add(move);
            DrawMove(move);

            await EndIfSomeoneWin();

            TemporaryPause = false;

            return true;
        }
        private async void AskForNewMoves()
        {
            while (GameInfo.StatusId!=10)
            {
                await Task.Delay(1000);
                if (TemporaryPause)
                    continue;
                var lastMove = await Api.GetLastMove(GameInfo.Id);
                if(lastMove is not null)
                {
                    if (GameInfo.Moves.Any() && GameInfo.Moves.Last().Equals(lastMove))
                        continue;
                    DrawMove(lastMove);
                    GameInfo.Moves.Add(lastMove);
                    await EndIfSomeoneWin();
                }
            }
        }
        private async Task EndIfSomeoneWin()
        {
            var winner = CheckWinning();
            if (winner is not null)
            {
                await Api.CloseTheGame(GameInfo.Id);
                GameInfo.StatusId = 10;
                SetEnabledBoard(false);
                if (winner == Api.AboutMe.PlayerId)
                    Log("Gratulacje, wygrałeś!");
                    //MessageBox.Show("Gratulacje, wygrałeś!");
                else
                    Log("Przegrałeś!");
                    //MessageBox.Show("Przegrałeś!");
            }

        }
        private void ClearBoard()
        {
            foreach(var field in Board)
            {
                field.Content = "";
                field.Foreground = Brushes.Black;
            }
        }
        private void SetEnabledBoard(bool enable)
        {
            foreach(var field in Board)
                field.IsEnabled = enable;
        }
        private bool IsValidMove(Coords coords)
        {
            return coords.x >= 0 && coords.x < BOARD_SIZE
                && coords.y >= 0 && coords.y < BOARD_SIZE;
        }
        private bool IsFreeSlot(Coords coords)
        {
            return !GameInfo.Moves.Any(g => g.X == coords.x && g.Y == coords.y);
        }
        private long? WhoseMove()
        {
            if (GameInfo.Moves?.Count > 0)
                return GameInfo.Players?.FirstOrDefault(p => p.PlayerId != GameInfo.Moves.Last().PlayerId)?.PlayerId;

            //it will be first move
            //2 =  invited role
            return GameInfo.Players?.FirstOrDefault(p => (p.RoleId & 2) == 2)?.PlayerId;
        }
        public long? CheckWinning()
        {
            if (GameInfo is null || GameInfo.Moves is null) return null;

            var moves = GameInfo.Moves;

            foreach (var move in moves)
            {
                foreach (Direction dir in Enum.GetValues(typeof(Direction)))
                {
                    var coords = new Coords(move.X, move.Y);
                    short counter;
                    for (counter = 0; counter <= 4; counter++)
                    {
                        GetNextCoords(ref coords, dir);
                        var nextMove = moves.Where(m => m.X == coords.x && m.Y == coords.y).FirstOrDefault();
                        if (nextMove is null || nextMove.PlayerId != move.PlayerId)
                            break;
                    }
                    if (counter >= 4)
                        return move.PlayerId;
                }
            }
            return null;
        }
        private enum Direction { RightUp, Right, RightDown, Down }
        private struct Coords
        {
            public Coords(short x, short y)
            {
                this.x = x;
                this.y = y;
            }
            public short x;
            public short y;
            public override string ToString()
            {
                return $"({x},{y})";
            }
        }
        private void GetNextCoords(ref Coords start, Direction direction)
        {
            switch (direction)
            {
                case Direction.RightUp:
                    start.x++;
                    start.y++;
                    break;
                case Direction.Right:
                    start.x++;
                    //start.y;
                    break;
                case Direction.RightDown:
                    start.x++;
                    start.y--;
                    break;
                case Direction.Down:
                    //start.x; 
                    start.y--;
                    break;
                default: break;
            }
        }
    }
}
