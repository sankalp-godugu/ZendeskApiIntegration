namespace ZendeskApiIntegration.Model.Responses
{
    public class Result
    {
        public int Id { get; set; }
    }

    public class ListSearchResultsResponse
    {
        public List<Result>? Results { get; set; }
    }
}
