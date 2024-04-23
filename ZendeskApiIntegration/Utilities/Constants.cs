namespace ZendeskApiIntegration.Utilities
{
    public static class Constants
    {
        public const string Ticket = "ticket";
        public const string User = "user";
        public const string Organization = "organization";
        public const int PageSize = 1000;
        public const string MeaTrainingCohort415 = @"C:\Users\Sankalp.Godugu\Downloads\4.15 MEA Training Cohort 4.12.24 Final.xlsx";
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

        // file paths
        public static readonly string ListOfEndUsersCurr = @$"C:\Users\Sankalp.Godugu\OneDrive - NationsBenefits\Documents\Business\Zendesk Integration\EndUserSuspension\{DateTime.Now.Date:M-dd}\EndUserSuspensionList_{DateTime.Now.Date:MM_dd_yyyy}.xlsx";
        public static readonly string ListOfEndUsersPrev = @$"C:\Users\Sankalp.Godugu\OneDrive - NationsBenefits\Documents\Business\Zendesk Integration\EndUserSuspension\{DateTime.Now.AddDays(-7):M-dd}\EndUserSUspensionList_{DateTime.Now.AddDays(-7).Date:MM_dd_yyyy}.xlsx";

        //column names
        public const string Name = "Name";
        public const string Email = "Email";
        public const string LastLoginAt = "Last Logged In";
        public const string Status = "Status";
        public const string EndUsers = "End Users";

        // roles
        public const string EndUser = "end-user";

        // orgs
        public const long OrgNations = 16807567180439;

        //testing
        public const string MyEmail = "sankalp.godugu@nationsbenefits.com";

        // timers
        public const int MaxAttempts = 5;
        public const int SleepTime = 5000;
    }
}
