using Newtonsoft.Json;

namespace ZendeskApiIntegration.Model.Requests
{
    public class UpdateManyTicketsRequest_Suspended
    {
        [JsonProperty("user")]
        public UserCustom? UserCustom { get; set; }
    }

    public class UserCustom
    {
        public bool Suspended { get; set; }
    }
}
