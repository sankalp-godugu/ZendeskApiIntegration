namespace ZendeskApiIntegration.Utilities
{
    public static class Constants
    {
        public const string CohortFilePath = @"C:\Users\Sankalp.Godugu\Downloads\Copy of April Surge - names locations e-mail UKG load date.xlsx";
        public static readonly Dictionary<long, string> Groups = new()
        {
            { 22245229281303, "Albertsons Marketplace" },
            { 18731640267543, "CSS"  },
            { 18153132673943, "DEV Data Request" },
            { 17859358579863, "DEV Prod Support" },
            { 18731658773143, "Grievances" },
            { 18880485044759, "Escalations" },
            { 18736796524183, "Mail Room" },
            { 18731646602263, "MCO Admin Portal" },
            { 19851565498519, "MCO Client / Agent Requests" },
            { 20499080460695, "MCO NationsMeals" },
            { 20111813580055, "Nations Meals" },
            { 18736987865751, "PERS" },
            { 18731644124439, "Supervisor Callbacks" },
            { 18737030342295, "Transportation" }
        };

        public static class FilePath
        {
            public static readonly string ListOfEndUsersNotified = @$"C:\Users\Sankalp.Godugu\OneDrive - NationsBenefits\Documents\Business\Zendesk Integration\EndUserSuspension\{DateTime.Now.Date:M-dd}\ListOfEndUsersNotified_{DateTime.Now.Date:M-dd}.xlsx";

            public static readonly string ListOfEndUsersSuspended = @$"C:\Users\Sankalp.Godugu\OneDrive - NationsBenefits\Documents\Business\Zendesk Integration\EndUserSuspension\{DateTime.Now.Date:M-dd}\ListOfEndUsersSuspended_{DateTime.Now.Date:M-dd}.xlsx";

            public static readonly string ListOfEndUsersNotifiedLastWeek = @$"C:\Users\Sankalp.Godugu\OneDrive - NationsBenefits\Documents\Business\Zendesk Integration\EndUserSuspension\{DateTime.Now.AddDays(-7).Date:M-dd}\ListOfEndUsersNotified_{DateTime.Now.AddDays(-7).Date:M-dd}.xlsx";
        }

        public static class Limits
        {
            public const int PageSize = 1000;
            public const int BulkCreateMembershipsBatchSize = 100;
            public const int MaxAttempts = 1;
            public const int SleepTime = 2400;
            public const int Timeout = 30000;
        }
        public class Sheets
        {
            public const string Failed = "FAILED";
            public const string Success = "SUCCESS";
            public const string EndUsersSuspended_Failed = "End Users Suspended - FAILED";
            public const string EndUsersSuspended_Success = "End Users Suspended - SUCCESS";
        }
        public static class Columns
        {
            public const string Name = "Name";
            public const string Email = "Email";
            public const string LastLoginAt = "Last Logged In";
            public const string Status = "Status";
            public const string EndUsers = "End Users";
            public const string Cohort = "Cohort";
            public static readonly string[] Headers = ["Organization", "End User", "Email", "Notification Status", "Timestamp"];
        }
        public static readonly Dictionary<string, string> TestUsers = new()
        {
            { "Sankalp Godugu", "sankalp.godugu@nationsbenefits.com" },
            { "Austin Stephens", "astephens@nationsbenefits.com"  },
            { "Judson Noel", "jnoel@nationsbenefits.com" },
        };
        public const string MemberSupportEmail = "https://membersupport.nationsbenefits.com/";

        public static class Emails
        {
            public const string EmailTestAustinPersonal = "ahstephens01@gmail.com";
            public const string EmailTestJudson = "judsonzdtest@nationsbenefits.com";
            public const string EmailTestJudson2 = "sir.j.noel@gmail.com";

            public const string MyEmail = "sankalp.godugu@nationsbenefits.com";
            public const string EmailNationsAustinStephens = "astephens@nationsbenefits.com";
            public const string EmailNationsDavidDandridge = "ddandridge@nationsbenefits.com";

            public const int Timeout = 30000;
        }

        public static class Users
        {
            public const string MyName = "Sankalp Godugu";
            public const string TestNameAustin = "Austin Stephens";
            public const string TestNameJudson = "Judson Noel";
        }

        public static class Types
        {
            public const string Ticket = "ticket";
            public const string User = "user";
            public const string Organization = "organization";
        }

        public static class Roles
        {
            // roles
            public const string EndUser = "end-user";
            public const string Agent = "agent";
            public const string Admin = "admin";
        }

        public static class Organizations
        {
            // orgs
            public const long Nations = 16807567180439;
        }

        public static class RecordStatuses
        {
            public const string Updated = "Updated";
        }

        public static class JobStatuses
        {
            public const string Completed = "completed";
            public const string Working = "working";
            public const string Failed = "failed";
            public const string Queued = "queued";
        }
    }
}
