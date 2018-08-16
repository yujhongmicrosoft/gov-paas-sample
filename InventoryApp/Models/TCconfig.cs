using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryApp.Models
{
    public class TCconfig
    {
        public Cosmos cosmos { get; set; }
    }
        
    public class Cosmos
    {
        public string Uri { get; set; }
        public string Key { get; set; }
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }
    }

}
