using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TrafficCaseApp.Models
{
    public class TCConfig
    {
        public CosmosConfig CosmosConfig { get; set; }
    }
        
    public class CosmosConfig
    {
        public string Uri { get; set; }
        public string Key { get; set; }
    }

}
