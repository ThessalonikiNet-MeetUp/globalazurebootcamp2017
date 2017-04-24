using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TestToSpeechDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(() => MainAsync());
            Console.ReadLine();
        }
        static async Task MainAsync()
        {
            var token = await GetAuthenticationToken("9541b8b2a18e46aaaf375d0144dc7293");

            // Say the fist line of the 'The Odyssey' in English - GB  
            var say = new List<string>();
            var English_GB = GenerateSSML("en-GB",
                                    "Female",
                                    "Microsoft Server Speech Text to Speech Voice (en-GB, Susan, Apollo) ",
                                    "Tell me, O muse, of that ingenious hero who travelled far and wide , after he had sacked the famous town of Troy.");
            say.Add(English_GB);

            // Say the fist line of the 'The Odyssey' in English - IN
            var English_IN = GenerateSSML("en-IN",
                                    "Male",
                                    "Microsoft Server Speech Text to Speech Voice (en-IN, Ravi, Apollo)",
                                    "Tell me, O muse, of that ingenious hero who travelled far and wide , after he had sacked the famous town of Troy.");
            say.Add(English_IN);

            // Say 'Guten Morgen!' German
            var German = GenerateSSML("de-DE",
                             "Male",
                             "Microsoft Server Speech Text to Speech Voice (de-DE, Stefan, Apollo)",
                             "Guten Morgen!");
            say.Add(German);

            // Say 'Hello' in Chinese 
            var Zh_CN = GenerateSSML("zh-CN",
                                    "Female",
                                    "Microsoft Server Speech Text to Speech Voice (zh-CN, Yaoyao, Apollo)",
                                    "你好");
            say.Add(Zh_CN);

            // Say something based on SSML
            var SpeechSynthesisMarkupLanguage =  new StreamReader("SSML.xml").ReadToEnd();
            say.Add(SpeechSynthesisMarkupLanguage);

            foreach (var item in say)
            {
                Console.WriteLine(item);

                var result = await Synthesize(token, item);

                PlayAudio(result);

                Thread.Sleep(3000);
            }

        }

        private static void PlayAudio(Stream result)
        {
            SoundPlayer p = new SoundPlayer();

            SoundPlayer player = new SoundPlayer(result);

            player.PlaySync();

            result.Dispose();
        }

        static string GenerateSSML(string locale, string gender, string name, string text)
        {
            var xmlDoc = new XDocument(
                              new XElement("speak",
                                  new XAttribute("version", "1.0"),
                                  new XAttribute(XNamespace.Xml + "lang", "en-US"),
                                  new XElement("voice",
                                      new XAttribute(XNamespace.Xml + "lang", locale),
                                      new XAttribute(XNamespace.Xml + "gender", gender),
                                      new XAttribute("name", name),
                                      text)));
            return xmlDoc.ToString();
        }
        static async Task<string> GetAuthenticationToken(string SubscriptionKey)
        {
            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "9541b8b2a18e46aaaf375d0144dc7293");

            var response = await client.PostAsync("https://api.cognitive.microsoft.com/sts/v1.0/issueToken", null);

            var token = await response.Content.ReadAsStringAsync();

            Console.WriteLine("Access token :\n" + token);

            return token;
        }

        static async Task<Stream> Synthesize(string token, string ssml)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + token);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/ssml+xml");
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Microsoft-OutputFormat", "riff-16khz-16bit-mono-pcm");
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Search-AppId", "07D3234E49CE426DAA29772419F436CA");
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Search-ClientID", "1ECFAE91408841A480F00935DC390960");
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Azure Global Bootcamp test client");

            var r = await client.PostAsync("https://speech.platform.bing.com/synthesize",
                new StringContent(ssml));

            Console.WriteLine("Synthesize result:\n" + r.StatusCode);
            Stream audiostream = await r.Content.ReadAsStreamAsync();
            return audiostream;
        }
    }
}
