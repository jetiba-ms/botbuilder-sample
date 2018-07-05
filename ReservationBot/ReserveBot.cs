using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Samples;
using Microsoft.Bot.Schema;
using PromptlyBot;

namespace ReservationBot
{
    public class BotConversationState : PromptlyBotConversationState<RootTopicState>
    {
    }

    public class BotUserState
    {
        public List<Reservation> Reservations { get; set; }
    }

    internal class ReserveBot : IBot
    {
        public async Task OnTurn(ITurnContext turnContext)
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var rootTopic = new RootTopic(turnContext);
                await rootTopic.OnTurn(turnContext);

            }
        }
    }
}