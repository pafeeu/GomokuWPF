using GomokuWPF.API.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Input;

namespace GomokuWPF.API
{
    public class MyApi
    {
        private HttpClient client;
        public List<string> Logs = new List<string>();
        public AccountResponse AboutMe;
        private static string UriApi = "api/";
        private static string UriAccount = UriApi+"account";
        private static string UriGames= UriApi+"games";
        private static string UriMoves= UriApi+"moves";
        private static string UriPlayers= UriApi+"players";
        public MyApi()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:5001/");
        }
        public async Task<bool> Login(string username, string password)
        {
            var response = await client.PostAsync(UriAccount+"/login", Serialize(new { username = username, password = password }));
            if (await IsSuccess(response))
            {
                var token = JsonConvert.DeserializeObject<TokenResponse>(await response.Content.ReadAsStringAsync());
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
                return await Me();
            }
            return false;
        }
        public async Task<bool> Register(string username, string password, string email)
        {
            var response = await client.PostAsync(UriAccount + "/register", Serialize(new { username = username, password = password, email = email }));
            if (await IsSuccess(response))
            {
                var info = JsonConvert.DeserializeObject<AuthResponse>(await response.Content.ReadAsStringAsync());

                return true;
            }
            return false;
        }
        public async Task<bool> Me()
        {
            var response = await client.GetAsync(UriAccount + "/me");
            if (await IsSuccess(response))
            {
                var accountInfo = JsonConvert.DeserializeObject<AccountResponse>(await response.Content.ReadAsStringAsync());
                AboutMe = accountInfo;
                return true;
            }
            return false;
        }
        public async Task<long?> CreateGame(long? invitedPlayerId = null)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            if (invitedPlayerId is not null) query.Set("invitedPlayerId", invitedPlayerId.ToString());
            var response = await client.PostAsync(UriGames + "?"+ query.ToString(),null);
            if (await IsSuccess(response))
            {
                var info = JsonConvert.DeserializeObject<ObjectCreatedResponse>(await response.Content.ReadAsStringAsync());
                return info.Id;
            }
            return null;
        }
        public async Task<List<GameResponse>> GetGames(long? playerId = null, short? gameStatus = null)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            if (playerId is not null) query.Set("playerId", playerId.ToString());
            if (gameStatus is not null) query.Set("status", gameStatus.ToString());
            var response = await client.GetAsync(UriGames+"?" + query.ToString());
            if (await IsSuccess(response))
            {
                var result = JsonConvert.DeserializeObject<List<GameResponse>>(await response.Content.ReadAsStringAsync());
                return result.OrderBy(x=>x.StatusId).ToList();
            }
            return new List<GameResponse>();
        }
        public async Task<GameDetailsResponse?> GetGameDetails(long gameId)
        {
            var response = await client.GetAsync(UriGames + "/" + gameId.ToString());
            if (await IsSuccess(response))
            {
                var result = JsonConvert.DeserializeObject<GameDetailsResponse>(await response.Content.ReadAsStringAsync());
                return result;
            }
            return null;
        }
        public async Task<bool> JoinToTheGame(long gameId)
        {
            var response = await client.PatchAsync(UriGames + "/" + gameId.ToString() + "/join", null);
            return await IsSuccess(response);
        }
        public async Task<bool> CloseTheGame(long gameId)
        {
            var response = await client.PatchAsync(UriGames + "/" + gameId.ToString() + "/end", null);
            return await IsSuccess(response);
        }
        public async Task<GameDetailsResponse.Move?> GetLastMove(long gameId)
        {
            var response = await client.GetAsync(UriMoves + "/" + gameId.ToString() + "/last");
            if (await IsSuccess(response))
            {
                var result = JsonConvert.DeserializeObject<GameDetailsResponse.Move>(await response.Content.ReadAsStringAsync());
                return result;
            }
            return null;
        }
        public async Task<long?> MakeMove(long gameId, short x, short y)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query.Set("x", x.ToString());
            query.Set("y", y.ToString());
            var response = await client.PostAsync(UriMoves + "/" + gameId.ToString() + "/make-at?" + query.ToString(), null);
            if (await IsSuccess(response))
            {
                var info = JsonConvert.DeserializeObject<ObjectCreatedResponse>(await response.Content.ReadAsStringAsync());
                return info.Id;
            }
            return null;
        }

        public async Task<List<PlayerResponse>> GetPlayers(string? playerName = null)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            if(!string.IsNullOrEmpty(playerName)) query.Set("name", playerName.ToString());
            var response = await client.GetAsync(UriPlayers + "?" + query.ToString());
            if (await IsSuccess(response))
            {
                var info = JsonConvert.DeserializeObject<List<PlayerResponse>>(await response.Content.ReadAsStringAsync());
                return info;
            }
            return new List<PlayerResponse>();
        }
        public void Logout()
        {
            client.DefaultRequestHeaders.Authorization = null;
        }
        private async Task<bool> IsSuccess(HttpResponseMessage response)
        {
            if(!response.IsSuccessStatusCode)
            {
                var message = "";
                dynamic content = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
                if (content is not null)
                {
                    var temp = (string?)content.message;
                    if (!string.IsNullOrEmpty(temp))
                        message += temp + " ";
                    temp = (string?)content.title;
                    if (!string.IsNullOrEmpty(temp))
                        message += temp + " ";
                }
                else
                    message = response.ReasonPhrase ?? "";
                Logs.Add(message);
                return false;
            }
            return true;
        }
        private static StringContent Serialize(object content)
        {
            return new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");
        }
    }
}
