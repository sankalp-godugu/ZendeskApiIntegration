using Newtonsoft.Json;

namespace ZendeskApiIntegration.Model
{
    public class CountUsersResponse
    {
        public Count? Count { get; set; }
    }

    public class Count
    {
        public int Value { get; set; }

        [JsonProperty("refreshed_at")]
        public string? RefreshedAt { get; set; }
    }
}
