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
    public class TranslationResult
    {
        public DetectedLanguage DetectedLanguage { get; set; }
        public TextResult SourceText { get; set; }
        public Translation[] Translations { get; set; }
    }
    public class DetectedLanguage
    {
        public string Language { get; set; }
        public float Score { get; set; }
    }

    public class TextResult
    {
        public string Text { get; set; }
        public string Script { get; set; }
    }

    public class Translation
    {
        public string Text { get; set; }
        public TextResult Transliteration { get; set; }
        public string To { get; set; }
        public Alignment Alignment { get; set; }
        public SentenceLength SentLen { get; set; }
    }

    public class Alignment
    {
        public string Proj { get; set; }
    }

    public class SentenceLength
    {
        public int[] SrcSentLen { get; set; }
        public int[] TransSentLen { get; set; }
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
        private static async Task TranslateAsync(string route, string textToTranslate)
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

            object[] body = new object[] { new { Text = textToTranslate } };
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
                TranslationResult[] deserializedOutput = JsonConvert.DeserializeObject<TranslationResult[]>(result);
                // Iterate over the deserialized results.
                foreach (TranslationResult o in deserializedOutput)
                {
                    // Print the detected input languge and confidence score.
                    Console.WriteLine("Detected input language: {0}\nConfidence score: {1}\n", o.DetectedLanguage.Language, o.DetectedLanguage.Score);
                    // Iterate over the results and print each translation.
                    foreach (Translation t in o.Translations)
                    {
                        Console.WriteLine("Translated to {0}: {1}", t.To, t.Text);
                    }
                }
            }
        }

        static async Task Main(string[] args)
        {
            string route = "/translate?api-version=3.0&to=de&to=it&to=ja&to=th";
            string textToTranslate = "Welcome to Microsoft Translator. Guess how many languages I speak!";
            await TranslateAsync(route, textToTranslate);
            Console.WriteLine("Press any key to continue.");
            Console.ReadKey();
        }        
    }
}
