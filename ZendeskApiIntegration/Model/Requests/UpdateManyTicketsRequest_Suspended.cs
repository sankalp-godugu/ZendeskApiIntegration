using Newtonsoft.Json;

namespace ZendeskApiIntegration.Model.Requests
{
    public class UpdateManyTicketsRequest_Suspended
    {
        [JsonProperty("user")]
        public required UserCustom UserCustom { get; set; }
    }

    public class UserCustom
    {
        public bool Suspended { get; set; }
    }
}
