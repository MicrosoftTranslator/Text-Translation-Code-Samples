using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Text_Translation_AAD
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
        //Replace TRANSLATOR_TEXT_ENDPOINT with your resource endpoint. Ex:  https://api.cognitive.microsofttranslator.com/
        private static readonly string Endpoint = "TRANSLATOR_TEXT_ENDPOINT";

        // AAD Authority URL 
        private const string AuthorityUrl = "https://login.windows.net/";

        //Replace TRANSLATOR_RESOURCE_URL with the target resource that is the recipient of the requested token.. Ex: https://cognitiveservices.azure.com/.default
        private const string ResourceUrl = "TRANSLATOR_RESOURCE_URL";

        //Replace TENANT_ID with the Azure Active Directory tenant (directory) Id of the service principal.
        private const string TenantId = "TENANT_ID";

        //Replace AAD_APPLICATION_ID with AAD application Id requesting the token.
        private const string AADApplicationId = "AAD_APPLICATION_ID";

        //Replace AAD_APPLICATION_SECRET with the secret of the AAD application Id requesting the token.
        private const string AADApplicationSecret = "AAD_APPLICATION_SECRET";

        //Replace RESOURCE_ID with the resource ID. Ex:/subscriptions/<your-subscription-id>/resourceGroups/<your-resource-group>/providers/MicrosoftCognitiveServices/accounts/<your-translator-resource>
        private const string ResourceId = "RESOURCE_ID";

        //Replace RESOURCE_REGION with the azure resource region
        private const string ResourceRegion = "RESOURCE_REGION";

        /// Demonstrates getting an access token and using the token to make Translate API call.
        private static async Task TransliterateAsync(string route, string inputText)
        {
            //Fetch the AAD Auth Token
            var aadTokenSource = new AADToken(TenantId, AuthorityUrl, ResourceUrl, AADApplicationId, AADApplicationSecret);
            var token = string.Empty;
            try
            {
                token = await aadTokenSource.GetAADAccessToken().ConfigureAwait(false);
            }
            catch (HttpRequestException)
            {
                switch (aadTokenSource.RequestStatusCode)
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
                request.RequestUri = new Uri(Endpoint + route);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.TryAddWithoutValidation("Authorization", token);
                request.Headers.TryAddWithoutValidation("Ocp-Apim-ResourceId", ResourceId);
                request.Headers.TryAddWithoutValidation("Ocp-Apim-Subscription-Region", ResourceRegion);

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
