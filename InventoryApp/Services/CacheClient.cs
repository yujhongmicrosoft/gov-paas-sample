using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrafficCaseApp.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace TrafficCaseApp.Services
{
    public class CacheClient : ICacheClient
    {
        private IDistributedCache cache;
        private TCConfig config;
        public List<string> statusKeys = new List<string>(new string[] { "new", "pending", "dropped", "closed" });

        public CacheClient(IDistributedCache cache, TCConfig config)
        {
            this.cache = cache;
            this.config = config;
        }

        public string GetStatus(string key)
        {
            return this.cache.GetString(key);
        }

        public void WriteStatus(string key, string val)
        {
            this.cache.SetString(key, val);
        }
    }
}
