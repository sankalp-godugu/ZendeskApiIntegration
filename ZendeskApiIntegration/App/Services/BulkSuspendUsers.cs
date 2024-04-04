using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZendeskApiIntegration.App.Services
{
    public class BulkSuspendUsers(IHttpClientFactory httpClientFactory)
    {
        public async Task Initialize()
        {
            var result = GetUsers(ConstructApiUrl());
            int temp = 1;

            static string ConstructApiUrl()
            {
                string baseUrl = "https://nationsbenefits.zendesk.com/api/v2/users/search?query=type:user";
                return baseUrl;
            }            
        }

        async Task<IEnumerable<User>?> GetUsers(string url)
        {
            try
            {
                HttpResponseMessage response = await httpClientFactory.CreateClient("Zendesk").GetAsync(url);
                string result = await response.Content.ReadAsStringAsync();
                //File.WriteAllText(@"C:\Users\Sankalp.Godugu\OneDrive - NationsBenefits\Documents\Business\Zendesk Integration\Zendesk\tempFile.json", result);
                return response.IsSuccessStatusCode ? JsonConvert.DeserializeObject<IEnumerable<User>>(result) : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }
    }
}
