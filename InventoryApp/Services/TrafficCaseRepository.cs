using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using TrafficCaseApp.Models;
using Microsoft.Azure.Documents;
using System.Net;
using Newtonsoft.Json;

namespace TrafficCaseApp.Services
{
    public class TrafficCaseRepository : ITrafficCaseRepository
    {
        private ICacheClient cacheClient;
        private DocumentClient docClient;
        private TCConfig config;

        public TrafficCaseRepository(DocumentClient docClient, TCConfig config, ICacheClient cacheClient)
        {
            this.docClient = docClient;
            this.config = config;
            this.cacheClient = cacheClient;
        }

        public async Task Initialize()
        {
            await CreateCollection();
            await InitializeStatusList();
        }

        public List<TrafficCase> GetCases()
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(this.config.CosmosConfig.DatabaseName, this.config.CosmosConfig.CollectionName);
            IQueryable<TrafficCase> trafficQuery = this.docClient.CreateDocumentQuery<TrafficCase>(
                collectionUri)
                .Where(f => f.id.ToString() != "statuslist");

            return trafficQuery.ToList();
        }

        public async Task InitializeStatusList()
        {
            IQueryable<Status> statusQuery = this.docClient.CreateDocumentQuery<Status>(UriFactory.CreateDocumentCollectionUri(this.config.CosmosConfig.DatabaseName, this.config.CosmosConfig.CollectionName)).Where(d => d.id == "statuslist");
            if (statusQuery.AsEnumerable().Count() == 0)
            {
                List<string> statusList = new List<string>(new string[] { "Case filed", "Case penalty pending", "Case dropped", "Case closed" });
                Status statusJson = new Status();
                statusJson.id = "statuslist";
                statusJson.statuses = statusList;
                await this.docClient.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(this.config.CosmosConfig.DatabaseName, this.config.CosmosConfig.CollectionName), statusJson);
            }
        }

        public List<string> GetStatuses()
        {
            IQueryable<Status> statusQuery = this.docClient.CreateDocumentQuery<Status>(UriFactory.CreateDocumentCollectionUri(this.config.CosmosConfig.DatabaseName, this.config.CosmosConfig.CollectionName)).Where(d => d.id == "statuslist");
            Status statuslist = new Status();
            statuslist.statuses = statusQuery.AsEnumerable().FirstOrDefault().statuses;
            var item = JsonConvert.SerializeObject(statuslist);
            this.cacheClient.WriteStatus("statuslist", JsonConvert.SerializeObject(statuslist));
            return JsonConvert.DeserializeObject<Status>(item).statuses;
        }

        public async Task CreateCollection()
        {
            await this.docClient.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(this.config.CosmosConfig.DatabaseName), new DocumentCollection { Id = this.config.CosmosConfig.CollectionName });
        }

        public async Task<String> CreateCase(TrafficCase trafficCase)
        {
            await this.docClient.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(this.config.CosmosConfig.DatabaseName, this.config.CosmosConfig.CollectionName), trafficCase);
            return ("Successfully created Case");
        }

        public async Task EditCase(TrafficCase trafficCase)
        {
            var doc = this.docClient.CreateDocumentQuery<Status>(UriFactory.CreateDocumentCollectionUri(this.config.CosmosConfig.DatabaseName, this.config.CosmosConfig.CollectionName)).Where(d => d.id == trafficCase.id.ToString()).AsEnumerable().SingleOrDefault();
            await this.docClient.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(this.config.CosmosConfig.DatabaseName, this.config.CosmosConfig.CollectionName, doc.id), trafficCase);
        }

        public async Task<TrafficCase> GetCase(string id)
        {
            
            var doc = await this.docClient.ReadDocumentAsync(UriFactory.CreateDocumentUri(this.config.CosmosConfig.DatabaseName, this.config.CosmosConfig.CollectionName, id));
            return (TrafficCase)(dynamic)doc.Resource;

        }
    }
}