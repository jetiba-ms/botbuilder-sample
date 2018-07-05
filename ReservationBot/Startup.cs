using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;


namespace ReservationBot
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(_ => Configuration);
            services.AddBot<ReserveBot>(options =>
            {
                options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);
                options.Middleware.Add(new ConversationState<BotConversationState>(new MemoryStorage()));
                options.Middleware.Add(new UserState<BotUserState>(new MemoryStorage()));
                //options.EnableProactiveMessages = true;
                //this is the code to manage Bot State with an Azure Storage
                //in the code at the moment, I choose to manage that in a custom way
                //IStorage dataStore = new CosmosDbStorage(new Uri(Environment.GetEnvironmentVariable("CosmosDbUri")), Environment.GetEnvironmentVariable("CosmosDbKey"), "VSTSDB", "ConversationHistory");
                //options.Middleware.Add(new ConversationState<TokenModel>(dataStore));

            });
            //services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseBotFramework();
            //app.UseMvc();
        }
    }
}