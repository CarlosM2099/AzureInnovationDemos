﻿using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using VstsRestAPI.Viewmodel.Queue;

namespace VstsRestAPI.Queues
{
    public class Queue : ApiServiceBase
    {
        public Queue(IConfiguration configuration) : base(configuration) { }
        private readonly ILog logger = LogManager.GetLogger("ErrorLog", "ErrorLog");
        /// <summary>
        /// Get Agent queue
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, int> GetQueues()
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    Dictionary<string, int> dicQueues = new Dictionary<string, int>();
                    QueueModel viewModel = new QueueModel();

                    using (var client = GetHttpClient())
                    {
                        string req = Configuration.UriString + Configuration.Project + "/_apis/distributedtask/queues?api-version=" + Configuration.VersionNumber;
                        HttpResponseMessage response = client.GetAsync(req).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            viewModel = response.Content.ReadAsAsync<QueueModel>().Result;
                            if (viewModel != null && viewModel.Value != null)
                            {
                                foreach (AgentQueueModel aq in viewModel.Value)
                                {
                                    dicQueues[aq.Name] = aq.Id;
                                }
                            }
                        }
                        else
                        {
                            var errorMessage = response.Content.ReadAsStringAsync();
                            string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                            this.LastFailureMessage = error;
                        }
                    }

                    return dicQueues;
                }
                catch (Exception ex)
                {
                    logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "CreateReleaseDefinition" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return new Dictionary<string, int>(); 
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return new Dictionary<string, int>();
        }
        /// <summary>
        /// Create Agent Queue by queue name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int CreateQueue(string name)
        {
            int retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    AgentQueueModel viewModel = new AgentQueueModel
                    {
                        Name = name
                    };

                    using (var client = GetHttpClient())
                    {
                        var jsonContent = new StringContent(JsonConvert.SerializeObject(viewModel), Encoding.UTF8, "application/json");
                        var method = new HttpMethod("POST");

                        var request = new HttpRequestMessage(method, Configuration.Project + "/_apis/distributedtask/queues?api-version=" + Configuration.VersionNumber) { Content = jsonContent };
                        var response = client.SendAsync(request).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            viewModel = response.Content.ReadAsAsync<AgentQueueModel>().Result;
                        }
                        else
                        {
                            var errorMessage = response.Content.ReadAsStringAsync();
                            string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                            this.LastFailureMessage = error;
                        }
                    }

                    return viewModel.Id;
                }
                catch (Exception ex)
                {
                    logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + "CreateReleaseDefinition" + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    LastFailureMessage = ex.Message + " ," + ex.StackTrace;
                    retryCount++;

                    if (retryCount > 4)
                    {
                        return 0;
                    }

                    Thread.Sleep(retryCount * 1000);
                }
            }
            return 0;
        }
    }
}
