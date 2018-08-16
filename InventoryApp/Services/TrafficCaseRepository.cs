using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using InventoryApp.Models;
using Microsoft.Azure.Documents;
using System.Net;


namespace InventoryApp.Services
{
    public class TrafficCaseRepository : ITrafficCaseRepository
    {
        private DocumentClient docClient;
        private TCconfig config;

        public TrafficCaseRepository(DocumentClient docClient, TCconfig config)
        {
            this.docClient = docClient;
            this.config = config;
        }

        public async Task<List<TrafficCase>> GetCases()
        {
            //make sure collection is created
            CreateCollection();
            var collectionUri = UriFactory.CreateDocumentCollectionUri(this.config.cosmos.DatabaseName, this.config.cosmos.CollectionName);
            var docs = await this.docClient.ReadDocumentFeedAsync(collectionUri, new FeedOptions { MaxItemCount = 20 });
            List<TrafficCase> caseList = new List<TrafficCase>();
            foreach (var d in docs)
            {
                caseList.Add(d.ToObject<TrafficCase>());
            }
            return caseList;
        }

        public async void CreateCollection()
        {
            await this.docClient.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(this.config.cosmos.DatabaseName), new DocumentCollection { Id = this.config.cosmos.CollectionName });
        }
        public async Task<String> CreateCase(TrafficCase trafficCase)
        {
            try
            {
                await this.docClient.ReadDocumentAsync(UriFactory.CreateDocumentUri(this.config.cosmos.DatabaseName, this.config.cosmos.CollectionName, trafficCase.Id.ToString()));
                return ("Error");
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await this.docClient.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(this.config.cosmos.DatabaseName, this.config.cosmos.CollectionName), trafficCase);
                    return ("Successfully created Case");
                }
                else
                {
                    return ("Error");
                }
            }
        }
        public async void EditCase(TrafficCase trafficCase)
        {
            await this.docClient.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(this.config.cosmos.DatabaseName, this.config.cosmos.CollectionName, trafficCase.Id.ToString()), trafficCase);
        }
        
        public TrafficCase GetCase(string id)
        {
            IQueryable<TrafficCase> familyQuery = this.docClient.CreateDocumentQuery<TrafficCase>(
                    UriFactory.CreateDocumentCollectionUri(this.config.cosmos.DatabaseName, this.config.cosmos.CollectionName))
                    .Where(f => f.Id == id);
            return familyQuery.First<TrafficCase>();
        }
    }
}
