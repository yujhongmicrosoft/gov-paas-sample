using System;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using TrafficCaseApp.Services;
using TrafficCaseApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace TrafficCaseApp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);
            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }
        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigurePOCO<TCConfig>(this.Configuration.GetSection("TCConfig"));
            //Add DB
            services.AddMvc(options => {
                options.Filters.Add(new RequireHttpsAttribute());
            });
            //Add migration for db
            //Add AAD authentication
            services.AddAuthentication(options => {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddOpenIdConnect(options => {
                options.Authority = Configuration["Authentication:AzureAd:AADInstance"] + Configuration["Authentication:AzureAd:TenantId"];
                options.ClientId = Configuration["Authentication:AzureAd:ClientId"];
                options.CallbackPath = Configuration["Authentication:AzureAd:CallbackPath"];
            });

            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = Configuration.GetConnectionString("RedisConnection");
                options.InstanceName = "master";
            });

            //Add Azure Storage
            var credentials = new StorageCredentials(Configuration["Storage:AccountName"], Configuration["Storage:AccountKey"]);
            Func<IServiceProvider, CloudStorageAccount> storageAcctFunc =
                 p => new CloudStorageAccount(credentials, "core.usgovcloudapi.net", true);
            services.AddTransient<CloudStorageAccount>(storageAcctFunc);
            services.AddTransient<CloudQueueClient>(p => p.GetService<CloudStorageAccount>().CreateCloudQueueClient());
            //Add CosmosConfigConfig
            Func<IServiceProvider, DocumentClient> CosmosConfigFunc =
                p => new DocumentClient(new Uri(Configuration["TCConfig:CosmosConfig:Uri"]), Configuration["TCConfig:CosmosConfig:Key"]);
            services.AddTransient<DocumentClient>(CosmosConfigFunc);
            //adding config as singleton
            services.AddTransient<ICacheClient, CacheClient>();
            services.AddTransient<ITrafficCaseRepository, TrafficCaseRepository>();
            services.AddTransient<IQueueClient, QueueClient>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var initTask = serviceScope.ServiceProvider.GetService<ITrafficCaseRepository>().Initialize();
                initTask.Wait();
                var initQueue = serviceScope.ServiceProvider.GetService<IQueueClient>().InitializeQueue();
                initQueue.Wait();
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseAuthentication();


            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Cases}/{action=Index}/{id?}");
            });
        }
    }
}
