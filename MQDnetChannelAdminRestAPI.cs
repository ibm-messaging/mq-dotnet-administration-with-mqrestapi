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
/* FILE NAME:      MQDnetChannelAdminRestAPI.cs                                          */
/* DESCRIPTION:    Basic example of invoking IBM MQ REST API Programmatically            */
/*                                                                                       */
/* How to Run:                                                                           */
/* dotnet MQDotnetRestAPI -qmgr QM1 -verb GET -channel SYSTEM.DEF.SENDER -certDetails    */
/*        C:\\Temp\\cert\\restapi\\user.p12|password                                     */
/*                                                                                       */
/* Parameters:                                                                           */
/* -qmgr ==> QueueManager                                                                */
/* -channel ==> channel name                                                             */
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

namespace MQDotnetRestAPI
{

    class MQDotnetRestAPI
    {
        private String userCredentials = "mqadmin:mqadmin";
        private String qmgr = "";
        private String channel = "";
        private String command = "";
        private readonly String v1httpbaseUrl = "http://localhost:9080/ibmmq/rest/v1/admin";
        private readonly String v1httpsbaserUrl = "https://localhost:9443/ibmmq/rest/v1/admin";
        private readonly String mqscHttpUrl = "http://localhost:9080/ibmmq/rest/v2/admin/action";
        private readonly String mqscHttpsUrl = "https://localhost:9443/ibmmq/rest/v2/admin/action";
        private String url = "";
        private String verb = "GET";
        private String certDetails = "";
        private String certPath = "";
        private String certPwd = "";
        enum RESTAPIVERB { GET, POST };
        static void Main(string[] args)
        {
            try
            {
                var restAPI = new MQDotnetRestAPI();
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
            channel = cmdLineArguments.ContainsKey("-channel") ? cmdLineArguments["-channel"] : "";
            command = cmdLineArguments.ContainsKey("-command") ? cmdLineArguments["-command"] : "";
            if (channel != "" && command != "")
            {
                Console.WriteLine("Both channel and command options were provided as arguments,hence exiting");
                Console.WriteLine("Provide either channel or command option");
                System.Environment.Exit(-1);
            }
            if (!String.IsNullOrEmpty(certDetails))
            {
                certPath = certDetails.Split('|')[0];
                certPwd = certDetails.Split('|')[1];
                if (!String.IsNullOrEmpty(command))
                    url = mqscHttpsUrl;
                else
                    url = v1httpsbaserUrl;
            }
            else
            {
                if (!String.IsNullOrEmpty(command))
                    url = mqscHttpUrl;
                else
                    url = v1httpbaseUrl;
            }
        }

        /// <summary>
        /// Based on the Options provided form the URL
        /// </summary>
        private void CreateURL()
        {
            if (!String.IsNullOrEmpty(qmgr))
                url = url + "/qmgr/" + qmgr.Trim();
            if (!String.IsNullOrEmpty(command))
                url += "/mqsc";

            if (!String.IsNullOrEmpty(channel))
            {
                url += "/channel/" + channel.Trim();
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
                Task postCall = Task.Run(() => POST(httpClient, url, command));
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

        /// <summary>
        /// POST
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="url"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        private static async Task POST(HttpClient httpClient, String url, String mqscCommand)
        {
            var parameterJSon = new Parameters { command = mqscCommand };
            var json = new RunCommand { type = "runCommand", parameters = parameterJSon };
            var jsonData = JsonSerializer.Serialize(json);
            Console.WriteLine(jsonData);
            var data = new StringContent(JsonSerializer.Serialize(json), Encoding.UTF8, "application/json");
            data.Headers.Add("-u", "mqadmin:mqadmin");
            data.Headers.Add("ibm-mq-rest-csrf-token", "value");
            var response = await httpClient.PostAsync(url, data);
            var resultStr = response.Content.ReadAsStringAsync().Result;
            if (!String.IsNullOrEmpty(resultStr))
                Console.WriteLine(resultStr);
            Console.WriteLine(response);
        }
    }

    /// <summary>
    /// Class used for creating JSON data which is used as an input for POST Calls
    /// </summary>
    public class RunCommand
    {
        public String type { get; set; }
        public Parameters parameters { get; set; }
    }
    public class Parameters
    {
        public String command { get; set; }
    }
}