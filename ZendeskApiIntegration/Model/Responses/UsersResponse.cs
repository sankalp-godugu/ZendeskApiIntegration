using Newtonsoft.Json;

namespace ZendeskApiIntegration.Model.Responses
{
    public class UsersResponse
    {
        [JsonProperty("results")]
        public List<User> Users { get; set; }

        [JsonProperty("next_page")]
        public string NextPage { get; set; }

        [JsonProperty("previous_page")]
        public string PreviousPage { get; set; }

        public int Count { get; set; }
    }
}
