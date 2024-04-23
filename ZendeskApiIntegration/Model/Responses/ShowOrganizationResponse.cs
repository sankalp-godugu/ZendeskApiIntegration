namespace ZendeskApiIntegration.Model.Responses
{
    public class ShowOrganizationResponse
    {
        public Organization? Organization { get; set; }
    }

    public class Organization
    {
        public long Id { get; set; }
        public string? Name { get; set; }
    }
}
