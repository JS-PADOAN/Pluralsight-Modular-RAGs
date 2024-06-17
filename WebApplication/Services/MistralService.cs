using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using WebApplication.Models;

namespace WebApplication.Services
{
    public class MistralService : ILLMService
    {
        private readonly IConfiguration configuration;

        public MistralService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task<string> GenerateSummarySentence(string userQuery, List<ResultSearch> results)
        {
            // let say i use the same prompt between mistral and OpenAI
            // TO be factorized ;-)

            var prompt = Properties.Resources.RAGPromptFinal; 

            prompt = prompt.Replace("<QUERY/>", userQuery);

            foreach (var result in results)
            {
                prompt = prompt.Replace("<IMAGE/>", $"name: '{result.id}', cosineSimilarityScore: {result.score}\r\n<IMAGE/>");
            }

            prompt = prompt.Replace("<IMAGE/>", "");

            return await generateAsync(prompt, this.configuration);
        }

        private static async Task<string> generateAsync(string userPrompt, IConfiguration conf)
        {
            HttpClient c = new HttpClient();
            HttpRequestMessage m = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new System.Uri(conf["MistralEndpoint"]),
                Content = new StringContent(
                       JsonConvert.SerializeObject(
                           new
                           {
                               model = "mistral-small-latest",
                               messages = new[]
                               {
                                    new
                                    {
                                        role="user", content = userPrompt
                                    }
                               },
                               temperature = 0.2,
                               top_p = 1,
                               max_tokens = 4000,
                               stream = false,
                               safe_prompt = false,
                               //random_seed = 1337
                           }
                   ), Encoding.UTF8, "application/json"
                   )
            };

            m.Headers.Authorization = new AuthenticationHeaderValue("Bearer", conf["MistralKey"]);

            var response = await c.SendAsync(m);

            string content = await response.Content.ReadAsStringAsync();

            //content = Properties.Resources.TextFile1;

            response.EnsureSuccessStatusCode();

            var res = JsonConvert.DeserializeObject<JObject>(content);

            string resContent = (string)res["choices"][0]["message"]["content"];

            return resContent;
        }
    }
}
