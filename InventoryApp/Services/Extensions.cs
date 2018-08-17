using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TrafficCaseApp.Services
{
    public static class Extensions
    {
        public static TConfig ConfigurePOCO<TConfig>(this IServiceCollection services, IConfiguration configuration) where TConfig : class, new()
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            var config = new TConfig();
            configuration.Bind(config);
            services.AddSingleton(config);
            return config;
        }

        public static SelectList ToSelectList(this List<string> list)
        {

           // return new SelectList(list, "Name", "Name");
            var selectitems = list.Select((r, index) => new SelectListItem { Text = r, Value = r}).ToList();
            return new SelectList(selectitems, "Value", "Text");
        }
    }
}
