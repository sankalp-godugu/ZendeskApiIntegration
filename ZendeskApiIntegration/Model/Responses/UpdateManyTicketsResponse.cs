namespace BulkMoveTickets.Responses
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class JobStatus
    {
        public required string id { get; set; }
        public required string message { get; set; }
        public required string progress { get; set; }
        public required List<UMTRResult> results { get; set; }
        public required string status { get; set; }
        public required string total { get; set; }
        public required string url { get; set; }
    }

    public class UMTRResult
    {
        public required string id { get; set; }
        public required string index { get; set; }
    }

    public class UpdateManyTicketsResponse
    {
        public required JobStatus job_status { get; set; }
    }


}
