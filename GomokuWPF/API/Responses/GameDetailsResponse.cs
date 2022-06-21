using System;
using System.Collections.Generic;

namespace GomokuWPF.API.Responses
{
    public class GameDetailsResponse
    {
        public long Id { get; set; }
        public short StatusId { get; set; }
        public string Status { get; set; }
        public List<Player> Players { get; set; }
        public List<Move> Moves { get; set; }
        public class Move
        {
            public long PlayerId { get; set; }
            public short X { get; set; }
            public short Y { get; set; }
            public DateTime Time { get; set; }
            public override string ToString()
            {
                return $"Gracz ({PlayerId}) zrobił ruch na ({X}, {Y})";
            }
            public bool Equals(Move move)
            {
                return this.X == move.X && this.Y == move.Y && this.PlayerId == move.PlayerId;
            }
        }
        public class Player
        {
            public long Id { get; set; }
            public long PlayerId { get; set; }
            public string Username { get; set; }
            public short RoleId { get; set; }
            public string Role { get; set; }
            public override string ToString()
            {
                return $"Gracz '{Username}' ({PlayerId}) {Role}";
            }
        }
        public override string ToString()
        {
            return $"Gra {Id}: '{Status}', gracze: \n\t{string.Join("\n\t",Players)}, \nruchy: \n\t{string.Join("\n\t", Moves)}";
        }
    }
}
