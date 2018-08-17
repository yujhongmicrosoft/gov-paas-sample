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
            var collectionUri = GetDocCollectionUri(CosmosInfo.CasesCollection);
            IQueryable<TrafficCase> trafficQuery = this.docClient.CreateDocumentQuery<TrafficCase>(
                collectionUri)
                .Where(f => f.Id.ToString() != "statuslist");

            return trafficQuery.ToList();
        }

        public async Task InitializeStatusList()
        {
            IQueryable<Status> statusQuery = this.docClient.CreateDocumentQuery<Status>(GetDocCollectionUri(CosmosInfo.CasesCollection)).Where(d => d.id == "statuslist");
            if (statusQuery.AsEnumerable().Count() == 0)
            {
                List<string> statusList = new List<string>(new string[] { "Case filed", "Case penalty pending", "Case dropped", "Case closed" });
                Status statusJson = new Status();
                statusJson.id = "statuslist";
                statusJson.statuses = statusList;
                await this.docClient.CreateDocumentAsync(GetDocCollectionUri(CosmosInfo.CasesCollection), statusJson);
            }
        }

        public List<string> GetStatuses()
        {
            IQueryable<Status> statusQuery = this.docClient.CreateDocumentQuery<Status>(GetDocCollectionUri(CosmosInfo.CasesCollection)).Where(d => d.id == "statuslist");
            Status statuslist = new Status();
            statuslist.statuses = statusQuery.AsEnumerable().FirstOrDefault().statuses;
            var item = JsonConvert.SerializeObject(statuslist);
            this.cacheClient.WriteStatus("statuslist", JsonConvert.SerializeObject(statuslist));
            return JsonConvert.DeserializeObject<Status>(item).statuses;
        }

        public async Task CreateCollection()
        {
            await this.docClient.CreateDocumentCollectionIfNotExistsAsync(GetDatabaseUri(), new DocumentCollection { Id = CosmosInfo.CasesCollection });
        }

        public async Task<String> CreateCase(TrafficCase trafficCase)
        {
            await this.docClient.CreateDocumentAsync(GetDocCollectionUri(CosmosInfo.CasesCollection), trafficCase);
            return ("Successfully created Case");
        }

        public async Task EditCase(TrafficCase trafficCase)
        {
            var doc = this.docClient.CreateDocumentQuery<Status>(GetDocCollectionUri(CosmosInfo.CasesCollection)).Where(d => d.id == trafficCase.Id.ToString()).AsEnumerable().SingleOrDefault();
            await this.docClient.ReplaceDocumentAsync(GetDocumentUri(CosmosInfo.CasesCollection, doc.id), trafficCase);
        }
        
        public async Task DeleteCase(string id)
        {
            var doc = this.docClient.CreateDocumentQuery<Status>(GetDocCollectionUri(CosmosInfo.CasesCollection)).Where(d => d.id == id.ToString()).AsEnumerable().SingleOrDefault();
            await this.docClient.DeleteDocumentAsync(GetDocumentUri(CosmosInfo.CasesCollection, doc.id));
        }

        public async Task<TrafficCase> GetCase(string id)
        {
            
            var doc = await this.docClient.ReadDocumentAsync(GetDocumentUri(CosmosInfo.CasesCollection, id));
            return (TrafficCase)(dynamic)doc.Resource;

        }

        #region Private Methods

        private static Uri GetDocCollectionUri(string collectionName)
        {
            return UriFactory.CreateDocumentCollectionUri(CosmosInfo.DbName, collectionName);
        }

        private static Uri GetDocumentUri(string collectionName, string docId)
        {
            return UriFactory.CreateDocumentUri(CosmosInfo.DbName, collectionName, docId);
        }

        private static Uri GetDatabaseUri()
        {
            return UriFactory.CreateDatabaseUri(CosmosInfo.DbName);
        }

        #endregion
    }
}