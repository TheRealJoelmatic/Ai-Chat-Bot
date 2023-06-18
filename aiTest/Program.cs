using System;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Drawing;
using Console = Colorful.Console;
using Colorful;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace aiTest
{
    class Program
    {
        public static bool g_loading;
        public static bool g_Inchat;
        public static string g_apiKey;
        public static string WhatsShouldGptBe;
        static string g_configFilePath = "config/config.json";
        static HttpClient g_client = new HttpClient();

        static async Task Main(string[] args)
        {
            g_loading = true;
            Task.Run(() => Program.loadingAnimation());
            Console.WriteLine("Starting");
            await setupAsync();
        }

        static async Task setupAsync()
        {
            try
            {
                // Check the api key

                if (!File.Exists(g_configFilePath))
                {
                    Console.WriteLine(g_configFilePath + "No file found making one now");
                    // File doesn't exist, so create a new one
                    string l_newConfig = @"{""apiKey"": ""API_KEY""}";

                    Directory.CreateDirectory("config");
                    File.WriteAllText(g_configFilePath, "" + l_newConfig);
                    Console.WriteLine(g_configFilePath + "File made");
                }


                Console.WriteLine("Reading API file");

                // File exists, so read its contents
                string l_json = System.IO.File.ReadAllText(g_configFilePath);

                JObject l_jsonObj = JObject.Parse(l_json);
                string l_apiKey = l_jsonObj["apiKey"].ToString();

                Console.WriteLine("Reading API file");
                if (l_apiKey == "API_KEY" || l_apiKey == "")
                {
                    Console.WriteLine("No valid key found");
                    string l_newApiKey = questions("API key", "(Open AI) Key: ");
                    l_apiKey = l_newApiKey;

                    string l_newConfig = "{\"apiKey\": \"" + l_newApiKey + "\"}";
                    File.WriteAllText(g_configFilePath, l_newConfig);
                }
                Console.WriteLine("Found API file");
                // Set up to Send requests

                g_client.BaseAddress = new Uri("https://api.openai.com/v1/");
                g_client.DefaultRequestHeaders.Add("Authorization", "Bearer " + l_apiKey);

                var content = new StringContent("{\"messages\": [{\"role\": \"system\", \"content\": \"You are a helpful assistant.\"}, {\"role\": \"user\", \"content\": \"" + "(This is a api test to Ping you) Reply pong!" + "\"}], \"model\": \"gpt-3.5-turbo-0613\"}", Encoding.UTF8, "application/json");

                HttpResponseMessage response = await g_client.PostAsync("chat/completions", content);

                Console.WriteLine($"Response Status Code: {response.StatusCode}");
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response Body: {responseBody}");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("API key is valid!");
                }
                else
                {
                    string l_newConfig = "{\"apiKey\": \"" + "API_KEY" + "\"}";
                    File.WriteAllText(g_configFilePath, l_newConfig);
                    ERROR("API key: " + l_apiKey + " is invalid!");
                }

                g_apiKey = l_apiKey;
                Console.WriteLine("Setup Done!");

                g_Inchat = true;
                g_loading = false;

                Program program = new Program();
                await program.ChatGpt();

            }
            catch (Exception l_ex)
            {
                ERROR(l_ex.ToString());
            }
        }

        public async Task ChatGpt()
        {
            Console.Clear();

            Console.WriteLine("This is program that demonstrates how to use the chatGPT api in c# (made by Joelmatic#8817)");
            Console.WriteLine("");
            Thread.Sleep(400);
            Console.Write("What do you want chat gpt to roleplay as? : ");
            WhatsShouldGptBe = Console.ReadLine();
            Console.Clear();

            if (WhatsShouldGptBe == "" || WhatsShouldGptBe == " ") WhatsShouldGptBe = "A helpful assistant";

            var font = FigletFont.Load("Fonts//larry3d.flf");
            Figlet figlet = new Figlet(font);

            Console.WriteLine(figlet.ToAscii("CHAT GPT"), Color.FromArgb(67, 144, 198));

            Console.WriteLine("Chat GPT loaded rolplaying as " + WhatsShouldGptBe + " Enjoy");
            while (g_Inchat)
            {
                Console.Write("You: ");
                string l_Message = Console.ReadLine();
                Console.WriteLine("");
                string response = await SendChatGPTRequest(l_Message);
                Console.WriteLine("Chat GPT: " + response);
                Console.WriteLine("");

                await Task.Delay(100);
            }
        }

        public async Task<string> SendChatGPTRequest(string message)
        {
            string l_apiUrl = "https://api.openai.com/v1/chat/completions";
            string l_apiKey = g_apiKey;

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {l_apiKey}");
                client.DefaultRequestHeaders.Add("User-Agent", "OpenAI-ChatGPT-Client");

                var content = new StringContent(
                    $"{{ \"messages\": [{{ \"role\": \"system\", \"content\": \"you are now a  {WhatsShouldGptBe} that will stay in character all the time no matter whats said and just play along.\" }}, {{ \"role\": \"user\", \"content\": \"{message}\" }}], \"model\": \"gpt-3.5-turbo-0613\" }}",
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync(l_apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Deserialize the API response
                    var responseObj = JsonConvert.DeserializeObject<dynamic>(responseContent);

                    // Access the "content" property from the response
                    string assistantResponse = responseObj.choices[0].message.content;

                    // Return the assistant's response content
                    return assistantResponse;
                }
                else
                {
                    ERROR($"ChatGPT API request failed: {response.StatusCode} - {responseContent}");
                    return null;
                }
            }
        }

        public static async Task loadingAnimation()
        {
            var font = FigletFont.Load("Fonts//larry3d.flf");
            Figlet figlet = new Figlet(font);

            int animationIndex = 0;
            int loadingTextLength = 0;
            bool isFirstIteration = true;

            while (g_loading)
            {
                Console.Clear();

                string loadingText = "Loading" + new string('.', animationIndex);
                loadingTextLength = Math.Max(loadingTextLength, loadingText.Length);

                if (!isFirstIteration)
                {
                    // Move the cursor to the beginning of the line
                    Console.SetCursorPosition(0, Console.CursorTop);
                }

                Console.WriteLine(figlet.ToAscii(loadingText), Color.FromArgb(67, 144, 198));
                Thread.Sleep(400);

                animationIndex = (animationIndex + 1) % 4;
                isFirstIteration = false;
            }

            // Clear the loading text at the end
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.WriteLine(new string(' ', loadingTextLength));
        }
        public static string questions(string Title, string Text)
        {
            g_loading = false;
            var font = FigletFont.Load("Fonts//larry3d.flf");
            Figlet figlet = new Figlet(font);
            Console.Clear();
            Console.WriteLine(figlet.ToAscii(Title), Color.FromArgb(67, 144, 198));
            Console.WriteLine(Text);

            string l_APIKEY = Console.ReadLine();

            return l_APIKEY;
        }
        public static void ERROR(string error)
        {
            g_loading = false;
            var font = FigletFont.Load("Fonts//larry3d.flf");
            Figlet figlet = new Figlet(font);

            Thread.Sleep(1200);
            Console.WriteLine(figlet.ToAscii("! error !"), Color.Red);
            Console.WriteLine(error);

            Environment.Exit(0);
        }
    }
}
