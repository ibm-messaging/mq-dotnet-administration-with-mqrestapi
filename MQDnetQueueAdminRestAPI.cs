/*****************************************************************************************/
/*                                                                                       */
/*                                                                                       */
/*  (c) Copyright IBM Corporation 2021                                                   */
/*                                                                                       */
/*  Licensed under the Apache License, Version 2.0 (the "License");                      */
/*  you may not use this file except in compliance with the License.                     */
/*  You may obtain a copy of the License at                                              */
/*                                                                                       */
/*  http://www.apache.org/licenses/LICENSE-2.0                                           */
/*                                                                                       */
/*  Unless required by applicable law or agreed to in writing, software                  */
/*  distributed under the License is distributed on an "AS IS" BASIS,                    */
/*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.             */
/*  See the License for the specific language governing permissions and                  */
/*  limitations under the License.                                                       */
/*                                                                                       */
/*                                                                                       */
/*****************************************************************************************/
/*****************************************************************************************/
/*                                                                                       */
/*                                                                                       */
/*                  IBM MQ REST API's                                                    */
/*                                                                                       */
/* FILE NAME:      MQDnetQueueAdminRestAPI.cs                                            */
/* DESCRIPTION:    Basic example of invoking IBM MQ REST API Programmatically            */
/*                                                                                       */
/* How to Run:                                                                           */
/* dotnet MQDotnetRestAPI -qmgr QM1 -verb GET
/ * -certDetails C:\\Temp\\cert\\restapi\\user.p12|password                              */
/*                                                                                       */
/* Parameters:                                                                           */
/* -qmgr ==> QueueManager                                                                */
/* -queue ==> QueueName                                                                  */
/* -certDetails ==> certificate keydatabase & password  separated with '|'               */
/* -verb  ==> REST API VERB(GET/POST)                                                    */
/* -userCredentials ==> username followed by password separated with ':'                 */
/*                                                                                       */
/*****************************************************************************************/

using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;

namespace MQDotnetQueueAdminRestApI
{
    class MQDotnetQueueAdminRestApI
    {
        private String userCredentials = "mqadmin:mqadmin";
        private String qmgr = "";
        private String queue = "";
        private readonly String v1httpbaseUrl = "http://localhost:9080/ibmmq/rest/v1/admin";
        private readonly String v1httpsbaserUrl = "https://localhost:9443/ibmmq/rest/v1/admin";
        private String url = "";
        private String verb = "GET";
        private String certDetails = "";
        private String certPath = "";
        private String certPwd = "";
        enum RESTAPIVERB { GET, POST, DELETE, PATCH };
        static void Main(string[] args)
        {
            try
            {
                var restAPI = new MQDotnetQueueAdminRestApI();
                restAPI.ParseArguments(args);
                restAPI.InvokeRestAPI();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Parse the command-line args
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public void ParseArguments(String[] args)
        {
            Dictionary<String, String> props = new Dictionary<string, string>();
            var cmdLineArguments = Enumerable.Range(0, args.Length / 2).ToDictionary(i => args[2 * i], i => args[2 * i + 1]);
            qmgr = cmdLineArguments.ContainsKey("-qmgr") ? cmdLineArguments["-qmgr"] : "";
            if (String.IsNullOrEmpty(qmgr))
            {
                Console.WriteLine("Queue Manager name not provided. Hence exiting");
                System.Environment.Exit(-1);
            }
            userCredentials = cmdLineArguments.ContainsKey("-userCredentials") ? cmdLineArguments["-userCredentials"] : "mqadmin:mqadmin";
            verb = cmdLineArguments.ContainsKey("-verb") ? cmdLineArguments["-verb"] : "GET";
            certDetails = cmdLineArguments.ContainsKey("-certDetails") ? cmdLineArguments["-certDetails"] : "";
            queue = cmdLineArguments.ContainsKey("-queue") ? cmdLineArguments["-queue"] : "";

            if (!String.IsNullOrEmpty(certDetails))
            {
                certPath = certDetails.Split('|')[0];
                certPwd = certDetails.Split('|')[1];
                url = v1httpsbaserUrl;
            }
            else url = v1httpbaseUrl;
        }

        /// <summary>
        /// Based on the Options provided form the URL
        /// </summary>
        private void CreateURL()
        {
            if (!String.IsNullOrEmpty(qmgr))
                url = url + "/qmgr/" + qmgr.Trim();

            if (!String.IsNullOrEmpty(queue))
            {
                if (verb == RESTAPIVERB.POST.ToString())
                    url += "/queue/";
                else
                    url += "/queue/" + queue.Trim();
            }
            Console.WriteLine("Invoking :" + url);
        }

        /// <summary>
        /// Invoke the REST API
        /// </summary>
        private void InvokeRestAPI()
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            // Please note the below certificate validation is only for testing purpose 
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            HttpClientHandler client = new HttpClientHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                }
            };

            // Add the certificate into the collection
            if (!String.IsNullOrEmpty(certDetails))
            {
                X509Certificate2 certificate = new X509Certificate2(certPath, certPwd);
                client.ClientCertificates.Add(certificate);
            }

            var httpClient = new HttpClient(client);
            var byteArray = Encoding.ASCII.GetBytes(userCredentials);
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            //Add the default headers
            AddDefaultHeaders(ref httpClient);
            // Form the URL 
            CreateURL();

            if (verb == RESTAPIVERB.POST.ToString())
            {
                Task postCall = Task.Run(() => POST(httpClient, url, queue));
                Console.WriteLine("Waiting for the POST call to finish...");
                postCall.Wait();
            }
            else
            {
                GET(httpClient);
            }
        }

        /// <summary>
        /// Default Headers for the API
        /// </summary>
        /// <param name="httpClient"></param>
        /// <returns></returns>
        private void AddDefaultHeaders(ref HttpClient httpClient)
        {
            httpClient.DefaultRequestHeaders.Add("-u", userCredentials);
            httpClient.DefaultRequestHeaders.Add("-H", "Content-Type:application/json");
            httpClient.DefaultRequestHeaders.Add("-H", "Content-Type:charset=UTF-8");
            httpClient.DefaultRequestHeaders.Add("-H", "ibm-mq-rest-csrf-token:value");
        }

        /// <summary>
        /// POST customized for the Queue only.
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        private static async Task POST(HttpClient httpClient, String url, String queueName)
        {
            var json = new JSONData { name = queueName };
            var jsonData = JsonSerializer.Serialize(json);
            Console.WriteLine(jsonData);
            var data = new StringContent(JsonSerializer.Serialize(json), Encoding.UTF8, "application/json");
            data.Headers.Add("-u", "mqadmin:mqadmin");
            data.Headers.Add("ibm-mq-rest-csrf-token", "value");
            var response = await httpClient.PostAsync(url, data);
            var resultStr = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine("POST call ended with :");
            Console.WriteLine(response);
            if (!String.IsNullOrEmpty(resultStr))
                Console.WriteLine(resultStr);
        }

        /// <summary>
        /// GET
        /// </summary>
        /// <param name="httpClient"></param>
        /// <returns></returns>
        private void GET(HttpClient httpClient)
        {
            var response = httpClient.GetAsync(url).Result;
            var resultStr = response.Content.ReadAsStringAsync().Result;
            if (!String.IsNullOrEmpty(resultStr))
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);
            Console.WriteLine(response);
        }
    }

    /// <summary>
    /// Class used for creating JSON data which is used as an input for POST Calls
    /// </summary>
    public class JSONData
    {
        public String name { get; set; }
    }
}