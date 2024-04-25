using Newtonsoft.Json;

namespace ZendeskApiIntegration.Model
{
    public class GroupMembershipsWrapper
    {
        [JsonProperty("group_memberships")]
        public List<GroupMembership> GroupMemberships = [];
    }

    public class GroupMembership
    {
        [JsonProperty("user_id")]
        public long UserId { get; set; }

        [JsonProperty("group_id")]
        public long GroupId { get; set; }
    }
}
