using Azure.AI.OpenAI;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace CSharpSimpleRAGConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: CSharpSimpleRAGConsole <inputFilePath>");
                return;
            }
            string inputFilePath = args[0];

            if (System.IO.File.Exists(inputFilePath))
            {
                string musicLibrary = System.IO.File.ReadAllText(inputFilePath);
                IConfigurationRoot config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
                string openAIEndpoint = config["AZURE_OPENAI_ENDPOINT"];
                string openAIDeploymentName = config["AZURE_OPENAI_GPT_NAME"];
                TokenCredential credential = new DefaultAzureCredential();
                AzureOpenAIClient openAIClient = new AzureOpenAIClient(new Uri(openAIEndpoint), credential);

                ChatClient chatClient = openAIClient.GetChatClient(openAIDeploymentName);

                List<ChatMessage> chatMessages = new List<ChatMessage>();
                SystemChatMessage systemChatMessage = new SystemChatMessage($"You are an AI librarian that helps answer questions about Nick's music library. Limit your answers to the following data which is in CSV format and the first row contains the column headings.{Environment.NewLine}###{Environment.NewLine}{musicLibrary}{Environment.NewLine}###");
                chatMessages.Add(systemChatMessage);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Welcome to Nick's music librarian. Please ask me anything you would like about Nick's music collection.");

                string message = string.Empty;
                Console.ForegroundColor = ConsoleColor.Yellow;
                message = Console.ReadLine();
                while (message != "bye")
                {
                    UserChatMessage userChatMessage = new UserChatMessage(message);
                    chatMessages.Add(userChatMessage);
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
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Input file does not exist.");
                Console.ResetColor();
            }

        }
    }
}
