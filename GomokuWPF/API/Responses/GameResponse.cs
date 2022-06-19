namespace GomokuWPF.API.Responses
{
    public record GameResponse
    {
        public long Id { get; init; }
        public int StatusId { get; init; }
        public string Status { get; init; }
        public int MovesNumber { get; init; }
        public int PlayersNumber { get; init; }
        public override string ToString()
        {
            return $"Gra {Id}: '{Status}' {(Status.Equals("New")&&PlayersNumber<2?"wolna":"zajeta")}";
        }
    }
}
