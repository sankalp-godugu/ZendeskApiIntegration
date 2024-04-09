using Newtonsoft.Json;

namespace ZendeskApiIntegration.Model.Responses
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class ExportSearchResultsResponse
    {
        [JsonProperty("results")]
        public List<User>? Users { get; set; }
        public List<Ticket>? Tickets { get; set; }

        public string? Facets { get; set; }
        public Meta? Meta { get; set; }
        public Links? Links { get; set; }
    }

    public class Meta
    {
        [JsonProperty("has_more")]
        public bool HasMore { get; set; }

        [JsonProperty("after_cursor")]
        public string? AfterCursor { get; set; }

        [JsonProperty("before_cursor")]
        public string? BeforeCursor { get; set; }
    }

    public class Links
    {
        public string? Prev { get; set; }
        public string? Next { get; set; }
    }
}
