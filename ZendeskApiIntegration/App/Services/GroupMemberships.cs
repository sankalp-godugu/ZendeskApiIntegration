using BulkCreateGroupMemberships.Requests;
using BulkCreateGroupMemberships.Responses;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace ZendeskApiIntegration.App.Services;

public class GroupMemberships(HttpClient client)
{
    public async Task Initialize()
    {
        // Main
        using HttpClient client = new();
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
        client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"sankalp.godugu@nationsbenefits.com/token:2T6Xd5Vpw6dwmKvUo8XZj1nPK4o5td9qheoSaER3")));

        string csvPath = ReadCsv();
        string apiUrl = "";

        Dictionary<long, string> groups = new()
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

        foreach (long groupId in groups.Select(g => g.Key))
        {
            bool isFirstIter = true;

            for (int i = 0; i < 400; i += 100)
            {
                IEnumerable<User> users = MapCsvToListOfUsers(csvPath, i + (isFirstIter ? 1 : 0));
                if (isFirstIter)
                {
                    isFirstIter = false;
                }

                apiUrl = ConstructApiUrl(users);
                UsersResponse result = await GetUsers(apiUrl);
                if (result?.users.Count > 0)
                {
                    GroupMembershipList groupMemberships = ConstructBulkGroupMembershipAssignmentJSON(result.users, groupId);
                    if (groupMemberships.group_memberships.Count > 0)
                    {
                        _ = await BulkCreateMemberships(groupMemberships);
                    }
                }
            }
        }
    }

    static string ReadCsv()
    {
        //System.Console.WriteLine("Enter full path of Csv file: ");
        return @"C:\Users\Sankalp.Godugu\OneDrive - NationsBenefits\Documents\Business\Zendesk Integration\Mini Surge 3.27 & 3.28.csv";//Console.ReadLine();
    }

    static IEnumerable<User> MapCsvToListOfUsers(string csvPath, int skip)
    {
        IEnumerable<User> users = File.ReadAllLines(csvPath)
        .Skip(skip) // skip the header rows
        .Take(100) // can only bulk assign 100 at a time
        .Select(col => col.Split(','))
        .Select(col => new User
        {
            email = col[0]
        });
        return users;
    }

    static string ConstructApiUrl(IEnumerable<User> users)
    {
        string baseUrl = "https://nationsbenefits.zendesk.com/api/v2/users/search?query=";
        string query = string.Empty;
        foreach (User user in users)
        {
            query += "user:" + user.email + " ";
        }
        query = query.Trim();
        return baseUrl + query;
    }

    async Task<UsersResponse?> GetUsers(string apiUrl)
    {
        HttpResponseMessage response = await client.GetAsync(apiUrl);
        string result = await response.Content.ReadAsStringAsync();
        //File.WriteAllText(@"C:\Users\Sankalp.Godugu\OneDrive - NationsBenefits\Documents\Business\Zendesk Integration\Zendesk\tempFile.json", result);
        return response.IsSuccessStatusCode ? JsonConvert.DeserializeObject<UsersResponse>(result) : null;
    }

    GroupMembershipList ConstructBulkGroupMembershipAssignmentJSON(List<User> users, long groupId)
    {
        GroupMembershipList groupMembershipList = new();
        foreach (User user in users)
        {
            if (user.role_type is not null)
            {
                groupMembershipList.group_memberships.Add(new GroupMembership
                {
                    user_id = user.id,
                    group_id = groupId
                });
            }
        }
        return groupMembershipList;
    }

    async Task<string> BulkCreateMemberships(GroupMembershipList groupMembershipList)
    {
        //HttpResponseMessage response = await client.PostAsJsonAsync("https://nationsbenefits.zendesk.com/api/v2/group_memberships/create_many", groupMembershipList);
        //var result = await response.Content.ReadAsStringAsync();

        string json = JsonConvert.SerializeObject(groupMembershipList);
        StringContent sc = new(json, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync("https://nationsbenefits.zendesk.com/api/v2/group_memberships/create_many", sc);
        string result = await response.Content.ReadAsStringAsync();

        //File.WriteAllText(@"C:\Users\Sankalp.Godugu\OneDrive - NationsBenefits\Documents\Business\Zendesk Integration\Zendesk\tempFile2.json", result);
        return response.IsSuccessStatusCode ? "success" : "error";
    }
}
