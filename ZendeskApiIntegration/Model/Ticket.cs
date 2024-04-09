using Newtonsoft.Json;
using ZendeskApiIntegration.Model.Responses;

namespace ZendeskApiIntegration.Model
{
    public class Ticket
    {
        [JsonProperty("url")]
        public string? Url { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("external_id")]
        public string? ExternalId { get; set; }

        [JsonProperty("via")]
        public Via? Via { get; set; }

        [JsonProperty("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [JsonProperty("generated_timestamp")]
        public long? GeneratedTimestamp { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("subject")]
        public string? Subject { get; set; }

        [JsonProperty("raw_subject")]
        public string? RawSubject { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("priority")]
        public string? Priority { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("recipient")]
        public string? Recipient { get; set; }

        [JsonProperty("requester_id")]
        public long? RequesterId { get; set; }

        [JsonProperty("submitter_id")]
        public long? SubmitterId { get; set; }

        [JsonProperty("assignee_id")]
        public string? AssigneeId { get; set; }

        [JsonProperty("organization_id")]
        public long? OrganizationId { get; set; }

        [JsonProperty("group_id")]
        public long? GroupId { get; set; }

        [JsonProperty("collaborator_ids")]
        public List<string>? CollaboratorIds { get; set; }

        [JsonProperty("follower_ids")]
        public List<string>? FollowerIds { get; set; }

        [JsonProperty("email_cc_ids")]
        public List<string>? EmailCcIds { get; set; }

        [JsonProperty("forum_topic_id")]
        public string? ForumTopicId { get; set; }

        [JsonProperty("problem_id")]
        public string? ProblemId { get; set; }

        [JsonProperty("has_incidents")]
        public bool? HasIncidents { get; set; }

        [JsonProperty("is_public")]
        public bool? IsPublic { get; set; }

        [JsonProperty("due_at")]
        public string? DueAt { get; set; }

        [JsonProperty("tags")]
        public List<string>? Tags { get; set; }

        [JsonProperty("custom_fields")]
        public List<CustomField>? CustomFields { get; set; }

        [JsonProperty("satisfaction_rating")]
        public string? SatisfactionRating { get; set; }

        [JsonProperty("sharing_agreement_ids")]
        public List<string>? SharingAgreementIds { get; set; }

        [JsonProperty("custom_status_id")]
        public long? CustomStatusId { get; set; }

        [JsonProperty("fields")]
        public List<Field>? Fields { get; set; }

        [JsonProperty("followup_ids")]
        public List<string>? FollowupIds { get; set; }

        [JsonProperty("ticket_form_id")]
        public long? TicketFormId { get; set; }

        [JsonProperty("brand_id")]
        public long? BrandId { get; set; }

        [JsonProperty("allow_channelback")]
        public bool? AllowChannelback { get; set; }

        [JsonProperty("allow_attachments")]
        public bool? AllowAttachments { get; set; }

        [JsonProperty("from_messaging_channel")]
        public bool? FromMessagingChannel { get; set; }

        [JsonProperty("result_type")]
        public string? ResultType { get; set; }
    }
}
