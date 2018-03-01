using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Ai;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Builder.Storage;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ReservationBot.Controllers;

namespace Microsoft.Bot.Samples
{
    [Route("api/[controller]")]
    public class MessagesController : BotController
    {
       public MessagesController(BotFrameworkAdapter adapter) : base(adapter) { }

        protected override async Task OnReceiveActivityAsync(IBotContext context)
        {
            if (context.Request.Type == ActivityTypes.Message)
            {
                var rootTopic = new RootTopic(context);
                await rootTopic.OnReceiveActivity(context);

            }
        }
    }
}




