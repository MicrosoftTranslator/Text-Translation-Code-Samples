using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Text_Translation_AzureToken
{
    /// <summary>
    /// The C# classes that represents the JSON returned by the Translator Text API.
    /// </summary>
    public class TransliterationResult
    {
        public string Text { get; set; }
        public string Script { get; set; }
    }

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
        private static async Task TransliterateAsync(string route, string inputText)
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

            object[] body = new object[] { new { Text = inputText } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                // Build the request.
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(endpoint + route);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Authorization", token);

                // Send the request and get response.
                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                // Read response as a string.
                string result = await response.Content.ReadAsStringAsync();
                TransliterationResult[] deserializedOutput = JsonConvert.DeserializeObject<TransliterationResult[]>(result);
                // Iterate over the deserialized results.
                foreach (TransliterationResult o in deserializedOutput)
                {
                    Console.WriteLine("Transliterated to {0} script: {1}", o.Script, o.Text);
                }
            }
        }

        static async Task Main(string[] args)
        {
            string route = "/transliterate?api-version=3.0&language=ja&fromScript=jpan&toScript=latn";
            string textToTransliterate = @"こんにちは";
            await TransliterateAsync(route, textToTransliterate);
            Console.WriteLine("Press any key to continue.");
            Console.ReadKey();
        }
    }
}
