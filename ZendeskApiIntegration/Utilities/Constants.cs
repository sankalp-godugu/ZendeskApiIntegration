namespace ZendeskApiIntegration.Utilities
{
    public static class Constants
    {
        public static readonly string Ticket = "ticket";
        public static readonly string User = "user";
        public static readonly string Organization = "organization";
        public static readonly int PageSize = 1000;

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
    }
}
