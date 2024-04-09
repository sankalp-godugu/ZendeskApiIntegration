namespace ZendeskApiIntegration.Model.Requests
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class UpdateManyTicketsRequest
    {
        public List<Ticket> tickets = [];
    }
}
