using Newtonsoft.Json;
using ZendeskApiIntegration.Model.Responses;

namespace ZendeskApiIntegration.App.Services;

public class BulkDeleteTickets(IHttpClientFactory httpClientFactory)
{
    public async Task Initialize()
    {
        // CALL EXPORT SEARCH RESULTS ENDPOINT to query tickets assigned to CSS group but are actually MCO tickets
        // 100 at a time

        // HTTP
        string searchQuery = "search/count?query=type:ticket group:18731646602263 requester:MEPtoZendesk@nationsbenefits.com created<=2023-12-14 -status:solved";
        PeriodicTimer timer = new(TimeSpan.FromSeconds(1));
        HttpClient client = httpClientFactory.CreateClient("Zendesk");
        HttpResponseMessage response = await client.GetAsync(searchQuery);
        string result = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Success");
        }
        searchQuery = "search?query=type:ticket group:18731646602263 requester:meptozendesk@nationsbenefits.com created<=2023-12-14 -status:solved";

        while (await timer.WaitForNextTickAsync())
        {
            try
            {
                response = await client.GetAsync(searchQuery);

                result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Success");
                }

                // CONVERT RESPONSE TO C# OBJECT
                ListSearchResultsResponse lsrr = JsonConvert.DeserializeObject<ListSearchResultsResponse>(result);

                if (lsrr == null || lsrr.Results == null || lsrr.Results.Count <= 0)
                {
                    continue;
                }

                string ids = "";

                // CONSTRUCT INPUT FROM API RESPONSE
                foreach (int ticketId in lsrr.Results.Select(t => t.Id))
                {
                    ids += ticketId + ",";
                }
                ids = ids.Trim(',');

                // CALL UPDATE MANY TICKETS ENDPOINT to reassign tickets to proper group
                // 100 at a time, so loop 10 times
                string destroyQuery = new("tickets/destroy_many?ids=" + ids);

                response = await client.DeleteAsync(destroyQuery);
                _ = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Success");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}