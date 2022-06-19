using System;
using System.Collections.Generic;

namespace GomokuWPF.API.Responses
{
    public record GameDetailsResponse
    {
        public long Id { get; init; }
        public short StatusId { get; init; }
        public string Status { get; init; }
        public List<Player> Players { get; init; }
        public List<Move> Moves { get; init; }
        public record Move
        {
            public long PlayerId { get; init; }
            public short X { get; init; }
            public short Y { get; init; }
            public DateTime Time { get; init; }
        }
        public record Player
        {
            public long Id { get; init; }
            public long PlayerId { get; init; }
            public string Username { get; init; }
            public short RoleId { get; init; }
            public string Role { get; init; }
        }
    }
}
