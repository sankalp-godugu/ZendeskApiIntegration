namespace ZendeskApiIntegration.Model.Responses
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Links
    {
        public required string next { get; set; }
        public required string prev { get; set; }
    }

    public class Meta
    {
        public required string after_cursor { get; set; }
        public required string before_cursor { get; set; }
        public required string has_more { get; set; }
    }

    public class ESRRResult
    {
        public required string created_at { get; set; }
        public required string @default { get; set; }
        public required string deleted { get; set; }
        public required string description { get; set; }
        public long id { get; set; }
        public required string name { get; set; }
        public required string result_type { get; set; }
        public required string updated_at { get; set; }
        public required string url { get; set; }
    }

    public class ExportSearchResultsResponse
    {
        public required string facets { get; set; }
        public required Links links { get; set; }
        public required Meta meta { get; set; }
        public required List<ESRRResult> results { get; set; }
    }


}
