namespace ZendeskApiIntegration.Model.Responses
{
    public class ShowJobStatusResponse
    {
        public JobStatus JobStatus { get; set; }
    }

    public class JobResult
    {
        public required string Action { get; set; }
        public long Id { get; set; }
        public required string Status { get; set; }
        public bool Success { get; set; }
    }
}
