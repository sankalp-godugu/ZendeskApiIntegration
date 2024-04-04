public class User
{
    public long id { get; set; }
    public  string url { get; set; }
    public  string name { get; set; }
    public  string email { get; set; }
    public  string created_at { get; set; }
    public  string updated_at { get; set; }
    public  string time_zone { get; set; }
    public  string iana_time_zone { get; set; }
    public  object phone { get; set; }
    public  object shared_phone_number { get; set; }
    public  object photo { get; set; }
    public int locale_id { get; set; }
    public  string locale { get; set; }
    public long? organization_id { get; set; }
    public  string role { get; set; }
    public bool verified { get; set; }
    public  string external_id { get; set; }
    public  List<object> tags { get; set; }
    public  string alias { get; set; }
    public bool active { get; set; }
    public bool shared { get; set; }
    public bool shared_agent { get; set; }
    public  string last_login_at { get; set; }
    public  object two_factor_auth_enabled { get; set; }
    public  object signature { get; set; }
    public  object details { get; set; }
    public  object notes { get; set; }
    public int? role_type { get; set; }
    public long? custom_role_id { get; set; }
    public bool moderator { get; set; }
    public  string ticket_restriction { get; set; }
    public bool only_private_comments { get; set; }
    public bool restricted_agent { get; set; }
    public bool suspended { get; set; }
    public bool chat_only { get; set; }
    public long? default_group_id { get; set; }
    public bool report_csv { get; set; }
    public UserFields user_fields { get; set; }
}

public class UserFields
{
    public  string department { get; set; }
    public  string title { get; set; }
}