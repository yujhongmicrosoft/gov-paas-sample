using Microsoft.Azure.Documents.Client;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrafficCaseApp.Models;

namespace TrafficCaseApp.Services
{
    public class ServiceFactory
    {
        private TCConfig config;
        public ServiceFactory(TCConfig config)
        {
            this.config = config;
        }
        public CloudStorageAccount CreateCloudStorageAccount() => 
            new CloudStorageAccount(
                new StorageCredentials(config., Configuration["Storage:AccountKey"]);
        Func<IServiceProvider, CloudStorageAccount> storageAcctFunc =
             p => new CloudStorageAccount(credentials, "core.usgovcloudapi.net", true);
        public DocumentClient CreateDocumentClient() => new DocumentClient(new Uri(this.config.CosmosConfig.Uri), this.config.CosmosConfig.Key);

    }
}
