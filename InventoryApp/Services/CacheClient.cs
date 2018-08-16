using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryApp.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace InventoryApp.Services
{
    public class CacheClient
    {
        private IDistributedCache cache;
        private TCconfig config;
        public List<string> statusKeys = new List<string>(new string[] { "new", "pending", "dropped", "closed" });

        public CacheClient(IDistributedCache cache, TCconfig config)
        {
            this.cache = cache;
            this.config = config;
        }
        public void InitializeStatuses()
        {
            this.cache.SetString("new", "Case filed");
            this.cache.SetString("pending", "Case penalty pending");
            this.cache.SetString("dropped", "Case dropped");
            this.cache.SetString("closed", "Case closed");
        }
        public List<string> GetStatuses()
        {
            List<string> statuses = new List<string>();
            while (statusKeys != null)
            {
                if (statusKeys.Count > 0)
                {
                    foreach (string statusKey in statusKeys)
                    {
                        string item = this.cache.GetString(statusKey);
                        if(item == null)
                        {
                            InitializeStatuses();
                            item = this.cache.GetString(statusKey);
                        }
                        List<String> substrings = item.Split(",").ToList();
                        string status = substrings[1];
                        statuses.Add(status);
                    }
                }
            }
            return statuses;
        }
    }
}
