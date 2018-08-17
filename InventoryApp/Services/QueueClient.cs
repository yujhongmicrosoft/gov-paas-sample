using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrafficCaseApp.Models;

namespace TrafficCaseApp.Services
{
    public class QueueClient : IQueueClient
    {
        private CloudQueueClient queueClient;

        public QueueClient(CloudQueueClient queueClient)
        {
            this.queueClient = queueClient;
        }
        
        public async Task AddCaseToQueue(TrafficCase trafficCase)
        {
            CloudQueue queue = this.queueClient.GetQueueReference("closedCases");
            await queue.CreateIfNotExistsAsync();

            if (trafficCase.Status == "Case closed")
            {
                var queueMsg = new CloudQueueMessage(JsonConvert.SerializeObject(trafficCase));
                await queue.AddMessageAsync(queueMsg);
            }
        }

        public async Task<List<TrafficCase>> GetClosedCases()
        {
            CloudQueue queue = this.queueClient.GetQueueReference("closedCases");
            await queue.CreateIfNotExistsAsync();
            var batch = await queue.GetMessagesAsync(3);
            List<TrafficCase> closedCaseList = new List<TrafficCase>();
            closedCaseList = batch.Select(msg => JsonConvert.DeserializeObject<TrafficCase>(msg.AsString)).ToList();
            return closedCaseList;
        }
    }
}
