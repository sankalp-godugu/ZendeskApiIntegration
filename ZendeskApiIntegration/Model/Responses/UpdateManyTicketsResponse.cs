using System.Text.Json.Serialization;

namespace ZendeskApiIntegration.Model.Responses
{
    public class JobStatus
    {
        public string Id { get; set; }
        [JsonPropertyName("job_type")]
        public string JobType { get; set; }
        public string Url { get; set; }
        public int Total { get; set; }
        public string Progress { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string Results { get; set; }
    }

    public class UpdateManyTicketsResponse
    {
        public JobStatus JobStatus { get; set; }
    }
}
