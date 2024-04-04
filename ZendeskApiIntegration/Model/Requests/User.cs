using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

public class User
{
    public User()
    {
        var settings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy() // Use SnakeCaseNamingStrategy for underscores
            }
        };
        JsonConvert.DefaultSettings = () => settings;
    }

    public long Id { get; set; }
    public string? Url { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }

    [JsonProperty(PropertyName ="created_at")]
    public string? CreatedAt { get; set; }

    [JsonProperty(PropertyName = "updated_at")]
    public string? UpdatedAt { get; set; }

    [JsonProperty(PropertyName = "time_zone")]
    public string? TimeZone { get; set; }

    [JsonProperty(PropertyName = "iana_time_zone")]
    public string? IanaTimeZone { get; set; }

    public string? Phone { get; set; }

    [JsonProperty(PropertyName = "shared_phone_number")]
    public string? SharedPhoneNumber { get; set; }

    public string? Photo { get; set; }

    [JsonProperty(PropertyName = "locale_id")]
    public int LocaleId { get; set; }

    public string? Locale { get; set; }

    [JsonProperty(PropertyName = "organization_id")]
    public long? OrganizationId { get; set; }

    public string? Role { get; set; }

    public bool Verified { get; set; }

    [JsonProperty(PropertyName = "external_id")]
    public string? ExternalId { get; set; }

    public List<object> Tags { get; set; }

    public string? Alias { get; set; }

    public bool Active { get; set; }

    public bool Shared { get; set; }

    [JsonProperty(PropertyName = "shared_agent")]
    public bool SharedAgent { get; set; }

    [JsonProperty(PropertyName = "last_login_at")]
    public string? LastLoginAt { get; set; }

    [JsonProperty(PropertyName = "two_factor_auth_enabled")]
    public string? TwoFactorAuthEnabled { get; set; }

    public string? Signature { get; set; }

    public string? Details { get; set; }

    public string? Notes { get; set; }

    [JsonProperty(PropertyName = "role_type")]
    public int? RoleType { get; set; }

    [JsonProperty(PropertyName = "custom_role_id")]
    public long? CustomRoleId { get; set; }

    public bool Moderator { get; set; }

    [JsonProperty(PropertyName = "ticket_restriction")]
    public string? TicketRestriction { get; set; }

    [JsonProperty(PropertyName = "only_private_comments")]
    public bool OnlyPrivateComments { get; set; }

    [JsonProperty(PropertyName = "restricted_agent")]
    public bool RestrictedAgent { get; set; }

    public bool Suspended { get; set; }

    [JsonProperty(PropertyName = "chat_only")]
    public bool ChatOnly { get; set; }

    [JsonProperty(PropertyName = "default_group_id")]
    public long? DefaultGroupId { get; set; }

    [JsonProperty(PropertyName = "report_csv")]
    public bool ReportCsv { get; set; }

    [JsonProperty(PropertyName = "user_fields")]
    public UserFields UserFields { get; set; }
}

public class UserFields
{
    public string? Department { get; set; }
    public string? Title { get; set; }
}