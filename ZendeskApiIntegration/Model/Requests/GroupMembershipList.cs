namespace ZendeskApiIntegration.Model.Requests
{
    public class GroupMembershipList
    {
        public List<GroupMembership> group_memberships = [];
    }

    public class GroupMembership
    {
        public long user_id { get; set; }
        public long group_id { get; set; }
    }
}
