using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TrafficCaseApp.Services
{
    public interface ICacheClient
    {
        string GetStatus(string key);
        void WriteStatus(string key, string val);

    }
}
