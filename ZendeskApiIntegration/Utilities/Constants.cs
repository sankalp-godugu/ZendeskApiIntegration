﻿namespace ZendeskApiIntegration.Utilities
{
    public static class Constants
    {
        public static readonly string Ticket = "ticket";
        public static readonly string User = "user";
        public static readonly string Organization = "organization";
        public static readonly int PageSize = 1000;
        public static readonly string MeaTrainingCohort415 = @"C:\Users\Sankalp.Godugu\Downloads\4.15 MEA Training Cohort 4.12.24 Final.xlsx";
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
        public static readonly string ListOfEndUsersPrev = @$"C:\Users\Sankalp.Godugu\OneDrive - NationsBenefits\Documents\Business\Zendesk Integration\EndUserSuspension\{DateTime.Now.Date:M-dd}\EndUserSUspensionList_{DateTime.Now.AddDays(-7).Date:MM_dd_yyyy}.xlsx";

        //column names
        public static readonly string Name = "Name";
        public static readonly string Email = "Email";
        public static readonly string LastLoginAt = "Last Logged In";
        public static readonly string Status = "Status";

        // roles
        public static readonly string EndUser = "end-user";
    }
}
