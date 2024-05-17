namespace ZendeskApiIntegration.Model.Responses
{
    public class Result
    {
        public int Id { get; set; }
    }

    public class ListSearchResultsResponse
    {
        public required List<Result> Results { get; set; }
    }
}
