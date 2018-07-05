using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using PromptlyBot;
using System.Text;
using PromptlyBot.Prompts;

namespace Microsoft.Bot.Samples
{
    public class ConfirmationTopicState : ConversationTopicState
    {
        public Confirmation confirmation = new Confirmation();
    }

    internal class ReservationConfirmationTopic : ConversationTopic<ConfirmationTopicState, Confirmation>
    {
        private const string CONFIRMATION_PROMPT = "confirmationPrompt";

        public ReservationConfirmationTopic(Reservation reservation) : base()
        {
            this.SubTopics.Add(CONFIRMATION_PROMPT, (object[] args) =>
            {
                var confirmationPrompt = new Prompt<string>();

                confirmationPrompt.Set
                    .OnPrompt((context, lastTurnReason) =>
                    {
                        var recapactivity = ReservationView.ReservationRecapCard(context, reservation);
                        context.SendActivity(recapactivity);

                        var activity = ReservationView.CreatedYesNoCard();
                        context.SendActivity(activity);
                    })
                    .Validator(new ReservationConfirmationValidator())
                    .MaxTurns(2)
                    .OnSuccess((context, value) =>
                    {
                        this.ClearActiveTopic();
                        this.State.confirmation.confirmationState = value;
                        this.OnTurn(context);
                    })
                    .OnFailure((context, reason) =>
                    {
                        this.ClearActiveTopic();
                        if (reason != null && reason == "toomanyattemps")
                        {
                            context.SendActivity("I'm sorry I'm having issues understanding you.");
                        }
                        this.OnFailure(context, reason);
                    });
                return confirmationPrompt;
            });
        }

        public override Task OnTurn(ITurnContext context)
        {
            if (HasActiveTopic)
            {
                ActiveTopic.OnTurn(context);
                return Task.CompletedTask;
            }
            if(State.confirmation.confirmationState == null)
            {
                this.SetActiveTopic(CONFIRMATION_PROMPT);
                this.ActiveTopic.OnTurn(context);
                return Task.CompletedTask;
            }

            this.OnSuccess(context, this.State.confirmation);

            return Task.CompletedTask;
        }
    }
}