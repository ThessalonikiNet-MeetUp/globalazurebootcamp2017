/**
Copyright (c) Microsoft Corporation
All rights reserved. 
MIT License
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the ""Software""), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
**/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Runtime.Serialization;
using System.Web;
using System.Threading;
using System.Net.Http;

namespace SpeechSample
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
            try
            {
                var result = Recognize(@"AudioInput.wav", token);

                Console.WriteLine(result);

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
            
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

        static  string Recognize(string audioFile, string token)
        {
            var requestUri = "https://speech.platform.bing.com/recognize?scenarios=smd&appid=D4D52672-91D7-4C74-8AD8-42B1D98141A5&locale=en-US&device.os=wp7&version=3.0&format=xml&instanceid=565D69FF-E928-4B7E-87DA-9A750B96D9E3&requestid="+ Guid.NewGuid().ToString();
         
            HttpWebRequest request = null;
            request = (HttpWebRequest)HttpWebRequest.Create(requestUri);
            request.SendChunked = true;
            request.Accept = @"application/json;text/xml";
            request.Method = "POST";
            request.ProtocolVersion = HttpVersion.Version11;
            request.Host = @"speech.platform.bing.com"; ;
            request.ContentType = @"audio/wav; codec=""audio/pcm""; samplerate=16000"; 
            request.Headers["Authorization"] = "Bearer " + token;

            FileStream fs = null;

            using (fs = new FileStream(audioFile, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = null;
                int bytesRead = 0;
                using (Stream requestStream = request.GetRequestStream())
                {
                    buffer = new Byte[checked((uint)Math.Min(1024, (int)fs.Length))];
                    while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        requestStream.Write(buffer, 0, bytesRead);
                    }
                    requestStream.Flush();
                }
            }

            string responseString;

            using (WebResponse response = request.GetResponse())
            {
                Console.WriteLine(((HttpWebResponse)response).StatusCode);

                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    responseString = sr.ReadToEnd();
                }
                
            }

            return responseString;           
            
        }
    }
}