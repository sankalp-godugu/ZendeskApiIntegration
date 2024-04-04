namespace ZendeskContactsProcessJob.Utilities
{
    public class ConfigConstants
    {
        public static string TokenUrlKey => "Zendesk:AppConfigurations:AccessTokenUrl";
        public static string GrantTypeKey => "Zendesk:AppConfigurations:GrantType";
        public static string ClientIdKey => "Zendesk:AppConfigurations:ClientId";
        public static string ClientSecretKey => "Zendesk:AppConfigurations:ClientSecret";
        public static string BaseUrlKey => "Zendesk:AppConfigurations:BaseUrl";
        public static string ContactListIdAetnaEnglishKey => "Zendesk:AppConfigurations:ContactListId:AetnaEnglish";
        public static string ContactListIdAetnaSpanishKey => "Zendesk:AppConfigurations:ContactListId:AetnaSpanish";
        public static string GetContacts => "Zendesk:ApiEndPoints:GetContacts";
        public static string RemoveContacts => "Zendesk:ApiEndPoints:RemoveContacts";
        public static string AddContacts => "Zendesk:ApiEndPoints:AddContacts";
        public static string UpdateNoDialContacts => "Zendesk:ApiEndPoints:UpdateContact";
        public static string UpdateAndDialContacts => "Zendesk:ApiEndPoints:UpdateAndDialContacts";
    }
}
