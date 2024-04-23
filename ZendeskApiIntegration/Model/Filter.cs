namespace ZendeskApiIntegration.Model
{
    public class Filter
    {
        public long OrgId { get; set; }
        public required string Role { get; set; }
        public required string LastLoginAt { get; set; }
    }
}
