using Azure.AI.OpenAI;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace CSharpChatCompletionConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IConfigurationRoot config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
            string openAIEndpoint = config["AZURE_OPENAI_ENDPOINT"];
            string openAIDeploymentName = config["AZURE_OPENAI_GPT_NAME"];
            TokenCredential credential = new DefaultAzureCredential();
            AzureOpenAIClient openAIClient = new AzureOpenAIClient(new Uri(openAIEndpoint), credential);

            ChatClient chatClient = openAIClient.GetChatClient(openAIDeploymentName);
            ChatCompletionOptions chatCompletionOptions = new ChatCompletionOptions()
            {
                Temperature = 1f,
                PresencePenalty = 0.0f,
                TopP = 0.95f
            };

            List<ChatMessage> chatMessages = new List<ChatMessage>();
            SystemChatMessage systemChatMessage = new SystemChatMessage("You are an AI assistant that helps people find information.");
            chatMessages.Add(systemChatMessage);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Welcome to Nick's chat bot. Please ask me anything.");

            string message = string.Empty;
            Console.ForegroundColor = ConsoleColor.Yellow;
            message = Console.ReadLine();
            while (message != "bye")
            {
                UserChatMessage userChatMessage = new UserChatMessage(message);
                chatMessages.Add(userChatMessage);
                //var completionResponse = chatClient.CompleteChat(chatMessages, chatCompletionOptions);
                var completionResponse = chatClient.CompleteChat(chatMessages);

                if (completionResponse.Value.FinishReason == ChatFinishReason.ContentFilter)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("The response was filtered due to your question triggering Azure OpenAI's content filtering system. Please rephrase your question and try again.");
                }
                else if (completionResponse.Value.FinishReason == ChatFinishReason.Length)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Your request exceeded the maximum number of tokens.");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(completionResponse.Value.Content[0].Text);
                    chatMessages.Add(new AssistantChatMessage(completionResponse.Value.Content[0].Text));
                }
                Console.ForegroundColor = ConsoleColor.Yellow;
                message = Console.ReadLine();
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Goodbye");
        }
    }
}
