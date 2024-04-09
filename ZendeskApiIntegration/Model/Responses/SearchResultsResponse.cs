using Newtonsoft.Json;

namespace ZendeskApiIntegration.Model.Responses
{
    public class SearchResultsResponse
    {
        [JsonProperty("results")]
        public List<Ticket>? Tickets { get; set; }
        public List<User>? Users { get; set; }

        public string? Facets { get; set; }

        [JsonProperty("next_page")]
        public string? NextPage { get; set; }

        [JsonProperty("previous_page")]
        public string? PreviousPage { get; set; }

        public int? Count { get; set; }
    }

    public class Via
    {
        [JsonProperty("channel")]
        public string? Channel { get; set; }

        [JsonProperty("source")]
        public Source? Source { get; set; }
    }

    public class Source
    {
        /*[JsonProperty("from")]
        public string? From { get; set; }

        [JsonProperty("to")]
        public string? To { get; set; }*/

        [JsonProperty("rel")]
        public string? Rel { get; set; }
    }

    public class CustomField
    {
        [JsonProperty("id")]
        public long? Id { get; set; }

        /*[JsonProperty("value")]
        public required string Value { get; set; }*/
    }

    public class Field
    {
        [JsonProperty("id")]
        public long? Id { get; set; }

        /*[JsonProperty("value")]
        public string? Value { get; set; }*/
    }
}