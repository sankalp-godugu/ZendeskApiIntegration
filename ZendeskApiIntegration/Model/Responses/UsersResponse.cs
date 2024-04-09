using Newtonsoft.Json;

namespace ZendeskApiIntegration.Model.Responses
{
    public class UsersResponse
    {
        [JsonProperty("results")]
        public required List<User> Users { get; set; }

        [JsonProperty("next_page")]
        public required string NextPage { get; set; }

        [JsonProperty("previous_page")]
        public required string PreviousPage { get; set; }

        public int Count { get; set; }
    }
}
