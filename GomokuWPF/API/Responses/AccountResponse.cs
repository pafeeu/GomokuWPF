using System.Collections.Generic;

namespace GomokuWPF.API.Responses
{
    public class AccountResponse
    {
        public long UserId { get; init; }
        public long PlayerId { get; init; }
        public string Username { get; init; }
        public string Email { get; init; }
        public List<string> Roles { get; init; }
        public char Symbol { get; init; }
    }
}
