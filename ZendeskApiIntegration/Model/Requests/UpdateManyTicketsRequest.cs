namespace BulkMoveTickets.Requests
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class UpdateManyTicketsRequest
    {
        public List<Ticket> tickets = new();
    }

    public class Ticket
    {
        public long id { get; set; }
        public long group_id { get; set; }
    }
}
