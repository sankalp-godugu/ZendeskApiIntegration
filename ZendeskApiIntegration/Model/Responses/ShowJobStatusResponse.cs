using System.Text.Json.Serialization;
using static ZendeskApiIntegration.Utilities.Constants;

namespace ZendeskApiIntegration.Model.Responses
{
    public class ShowJobStatusResponse
    {
        public JobStatus JobStatus { get; set; }
    }

    public class JobStatus
    {
        public string? Id { get; set; }
        [JsonPropertyName("job_type")]
        public string? JobType { get; set; }
        public string? Url { get; set; }
        public int? Total { get; set; }
        public int? Progress { get; set; }
        public string? Status { get; set; }
        public string? Message { get; set; }
        public List<JobResult>? Results { get; set; }
    }

    public class JobResult
    {
        public string Action { get; set; }
        public long Id { get; set; }
        public string Status { get; set; }
        public bool Success { get; set; }
        public string? SuspensionStatus => Success ? UserStatuses.Suspended : UserStatuses.Active;
    }
}
