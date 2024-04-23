namespace ZendeskApiIntegration.Model.Responses
{
    public class ShowJobStatusResponse
    {
        public class JobStatus
        {
            public string Id { get; set; }
            public string Message { get; set; }
            public int Progress { get; set; }
            public List<JobResult> Results { get; set; }
            public string Status { get; set; }
            public int Total { get; set; }
            public string Url { get; set; }
        }

        public class JobResult
        {
            public string Action { get; set; }
            public int Id { get; set; }
            public string Status { get; set; }
            public bool Success { get; set; }
        }
    }
}
