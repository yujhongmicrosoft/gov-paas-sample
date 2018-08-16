using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryApp.Services
{
    public interface ICacheClient
    {
        List<String> GetStatuses();
        void InitializeStatuses();

    }
}
