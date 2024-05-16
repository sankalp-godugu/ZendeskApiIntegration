using Newtonsoft.Json;

namespace ZendeskApiIntegration.Model.Responses
{
    public class ShowOrganizationsResponse
    {
        public int Count { get; set; }
        [JsonProperty("next_page")]
        public string? NextPage { get; set; }
        public List<Organization> Organizations { get; set; }
        [JsonProperty("previous_page")]
        public string? PreviousPage { get; set; }
    }
}
