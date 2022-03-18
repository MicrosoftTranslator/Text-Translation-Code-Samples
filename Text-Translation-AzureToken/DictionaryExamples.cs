using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Text_Translation_AzureToken
{
    class Program
    {
        // Replace YOUR-RESOURCE-KEY with Key1 or Key2 from Azure portal's Resource Management keys
        private static readonly string resourceKey = "YOUR-RESOURCE-KEY";

        // Replace YOUR_RESOURCE_LOCATION with the location used while setting up Azure subscription, 
        // Look in Azure portal->overview ex: eastus, westus, global
        private static readonly string location = "YOUR_RESOURCE_LOCATION";

        //Replace TRANSLATOR_TEXT_ENDPOINT with your resource endpoint
        private static readonly string endpoint = "TRANSLATOR_TEXT_ENDPOINT";

        /// Demonstrates getting an access token and using the token to make Translate API call.
        private static async Task DictionaryExamplesAsync(string route, string text, string translation)
        {
            //Fetch the Azure Auth Token
            var authTokenSource = new AzureAuthToken(resourceKey, location);
            var token = string.Empty;
            try
            {
                token = await authTokenSource.GetAccessTokenAsync();
            }
            catch (HttpRequestException)
            {
                switch (authTokenSource.RequestStatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        Console.WriteLine("Request to token service is not authorized (401). Check that the Azure subscription key is valid.");
                        break;
                    case HttpStatusCode.Forbidden:
                        Console.WriteLine("Request to token service is not authorized (403). For accounts in the free-tier, check that the account quota is not exceeded.");
                        break;
                }
                throw;
            }

            System.Object[] body = new System.Object[] { new { Text = text, Translation = translation } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                // Build the request.
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(endpoint + route);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Authorization", token);

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(responseBody), Formatting.Indented);

                Console.OutputEncoding = UnicodeEncoding.UTF8;
                Console.WriteLine(result);
            }
        }

        static async Task Main(string[] args)
        {
            string route = "/dictionary/examples?api-version=3.0&from=en&to=es";
            string text = "Shark";
            string translation = "tiburón";

            await DictionaryExamplesAsync(route, text, translation);
            Console.WriteLine("Press any key to continue.");
            Console.ReadKey();
        }
    }
}
