using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Ai;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Builder.Storage;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Samples
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(_ => Configuration);
            services.AddMvc();

            services.AddSingleton<BotFrameworkAdapter>(serviceProvider =>
            {

                string applicationId = Configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value;
                string applicationPassword = Configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppPasswordKey)?.Value;

                return new BotFrameworkAdapter(applicationId, applicationPassword)
                    .Use(new UserStateManagerMiddleware(new MemoryStorage()))
                    .Use(new ConversationStateManagerMiddleware(new MemoryStorage()));
                   // .Use(new LuisRecognizerMiddleware("1b5b859f-d818-413e-9832-8c4f3ce1e1e8", "ee328ee60011452aba369309895f6cb5"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMvc();
        }
    }
}