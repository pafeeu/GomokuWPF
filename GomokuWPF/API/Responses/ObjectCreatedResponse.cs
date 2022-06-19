namespace GomokuWPF.API.Responses
{
    public class ObjectCreatedResponse
    {
        public ObjectCreatedResponse(long id)
        {
            Id = id;
        }

        public long Id { get; set; }
    }
}
