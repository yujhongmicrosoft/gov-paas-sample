using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryApp.Models;

namespace InventoryApp.Services
{
    public interface ITrafficCaseRepository
    {
        Task<List<TrafficCase>> GetCases();
        void CreateCollection();
        Task<String> CreateCase(TrafficCase trafficCase);
        void EditCase(TrafficCase trafficCase);
        TrafficCase GetCase(string id);

    }
}
