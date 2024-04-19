using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ZendeskApiIntegration.Model;

public class User
{
    public User()
    {
        JsonSerializerSettings settings = new()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy() // Use SnakeCaseNamingStrategy for underscores
            }
        };
        JsonConvert.DefaultSettings = () => settings;
    }

    public long Id { get; set; }
    public long EEID { get; set; }
    public string? Url { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }

    [JsonProperty(PropertyName = "created_at")]
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

    public Photo? Photo { get; set; }

    [JsonProperty(PropertyName = "locale_id")]
    public int LocaleId { get; set; }

    public string? Locale { get; set; }

    [JsonProperty(PropertyName = "organization_id")]
    public long? OrganizationId { get; set; }

    public string? Role { get; set; }

    public bool Verified { get; set; }

    [JsonProperty(PropertyName = "external_id")]
    public string? ExternalId { get; set; }

    public List<string>? Tags { get; set; }

    public string? Alias { get; set; }

    public bool? Active { get; set; }

    public bool? Shared { get; set; }

    [JsonProperty(PropertyName = "shared_agent")]
    public bool? SharedAgent { get; set; }

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

    public bool? Moderator { get; set; }

    [JsonProperty(PropertyName = "ticket_restriction")]
    public string? TicketRestriction { get; set; }

    [JsonProperty(PropertyName = "only_private_comments")]
    public bool? OnlyPrivateComments { get; set; }

    [JsonProperty(PropertyName = "restricted_agent")]
    public bool? RestrictedAgent { get; set; }

    public bool? Suspended { get; set; }

    [JsonProperty(PropertyName = "chat_only")]
    public bool? ChatOnly { get; set; }

    [JsonProperty(PropertyName = "default_group_id")]
    public long? DefaultGroupId { get; set; }

    [JsonProperty(PropertyName = "report_csv")]
    public bool? ReportCsv { get; set; }

    [JsonProperty(PropertyName = "user_fields")]
    public UserFields? UserFields { get; set; }

    // custom fields
    public string? Status => Suspended == true ? "Suspended" : "Active";
    public bool ShouldSuspend => DateTime.Parse(LastLoginAt) > DateTime.Now.AddMonths(-1);
}

public class UserFields
{
    public string? Department { get; set; }
    public string? Title { get; set; }
}

public class MediaItem
{
    public string? Url { get; set; }
    public string? Id { get; set; }

    [JsonProperty("file_name")]
    public string? FileName { get; set; }

    [JsonProperty("content_url")]
    public string? ContentUrl { get; set; }

    [JsonProperty("mapped_content_url")]
    public string? MappedContentUrl { get; set; }

    [JsonProperty("content_type")]
    public string? ContentType { get; set; }

    public string? Size { get; set; }

    public string? Width { get; set; }

    public string? Height { get; set; }

    public bool? Inline { get; set; }

    public bool? Deleted { get; set; }
}

public class Photo : MediaItem
{
    public required List<Thumbnail> Thumbnails { get; set; }
}

public class Thumbnail : MediaItem
{
    // No additional properties specific to Thumbnail class
}