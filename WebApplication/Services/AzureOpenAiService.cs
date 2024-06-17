using Azure.AI.OpenAI;
using Azure;
using WebApplication.Models;

namespace WebApplication.Services
{
    public class AzureOpenAiService : ILLMService
    {
        private readonly IConfiguration configuration;

        public AzureOpenAiService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public async Task<string> GenerateSummarySentence(string userQuery, List<ResultSearch> results)
        {
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
            OpenAIClient client = new OpenAIClient(new Uri(conf["AzureOpenAIEndpoint"]), new AzureKeyCredential(conf["AzureOpenAIKey"]));

            Response<ChatCompletions> responseWithoutStream = await client.GetChatCompletionsAsync("deploy1",
            new ChatCompletionsOptions()
            {
                Messages =
              {
                new ChatMessage(ChatRole.System, @"You are an AI assistant that helps people find information."),
                new ChatMessage(ChatRole.User, userPrompt),
              },
                Temperature = (float)0.7,
                MaxTokens = 800,

                NucleusSamplingFactor = (float)0.95,
                FrequencyPenalty = 0,
                PresencePenalty = 0,
            });


            ChatCompletions response = responseWithoutStream.Value;

            return response.Choices[0].Message.Content;
        }
    }
}
