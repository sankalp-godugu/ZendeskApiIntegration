using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZendeskApiIntegration.Model.Requests
{
    public class UpdateManyTicketsRequest_Suspended
    {
        [JsonProperty("user")]
        public UserCustom UserCustom { get; set; }
    }

    public class UserCustom
    {
        public bool Suspended { get; set; }
    }
}
