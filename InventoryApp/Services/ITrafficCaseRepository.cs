using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrafficCaseApp.Models;

namespace TrafficCaseApp.Services
{
    public interface ITrafficCaseRepository
    {
        Task Initialize();
        List<TrafficCase> GetCases();
        Task InitializeStatusList();
        List<string> GetStatuses();
        Task CreateCollection();
        Task<String> CreateCase(TrafficCase trafficCase);
        Task EditCase(TrafficCase trafficCase);
        Task<TrafficCase> GetCase(string id);
        Task DeleteCase(string id);

    }
}
