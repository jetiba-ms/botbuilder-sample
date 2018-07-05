using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Core.Extensions;
using PromptlyBot;
using Microsoft.Bot.Samples;

namespace ReservationBot
{
    public class RootTopicState : ConversationTopicState
    {

    }

    internal class RootTopic : TopicsRoot<BotConversationState, RootTopicState>
    {
        private const string ADD_RESERVATION_TOPIC = "addReservationTopic";
        private const string DELETE_RESERVATION_TOPIC = "deleteReservationTopic";

        private const string USER_STATE_RESERVATION = "Reservations";

        public RootTopic(ITurnContext context) : base(context)
        {
            if (context.GetUserState<BotUserState>().Reservations == null)
            {
                context.GetUserState<BotUserState>().Reservations = new List<Reservation>();
            }

            this.SubTopics.Add(ADD_RESERVATION_TOPIC, (object[] args) =>
            {
                var addReservationTopic = new AddReservationTopic();

                addReservationTopic.Set
                .OnSuccess((ctx, reservation) =>
                {
                    this.ClearActiveTopic();
                    if(reservation.Confirmed == "yes")
                    {
                        ctx.GetUserState<BotUserState>().Reservations.Add(reservation);
                        context.SendActivity($"Reservation added!");
                    }
                    else
                    {
                        context.SendActivity("you deleted the reservation");
                    }
                    
                })
                .OnFailure((ctx, reason) =>
                {
                    this.ClearActiveTopic();
                    context.SendActivity("It fails for some reasons");
                    this.ShowDefaultMessage(context);
                });

                return addReservationTopic;

            });               
        }

        private void ShowDefaultMessage(ITurnContext context)
        {
            context.SendActivity("write 'add reservation' to start with a new reservation");
        }

        public override Task OnTurn(ITurnContext context)
        {
            if ((context.Activity.Type == ActivityTypes.Message) && (context.Activity.AsMessageActivity().Text.Length > 0))
            {
                var message = context.Activity.AsMessageActivity();
                
                //I can use LUIS here!

                // If the user wants to change the topic of conversation...
                if (message.Text.ToLowerInvariant() == "add reservation")
                {
                    // Set the active topic and let the active topic handle this turn.
                    this.SetActiveTopic(ADD_RESERVATION_TOPIC)
                            .OnTurn(context);
                    return Task.CompletedTask;
                }

                //TODO: implement "delete alarm topic"
                //if (message.Text.ToLowerInvariant() == "delete alarm")
                //{
                //    this.SetActiveTopic(DELETE_ALARM_TOPIC)
                //        .OnReceiveActivity(context);
                //    return Task.CompletedTask;
                //}

                if (message.Text.ToLowerInvariant() == "show reservation")
                {
                    this.ClearActiveTopic();

                    ReservationView.ShowReservations(context, context.GetUserState<BotUserState>().Reservations);
                    return Task.CompletedTask;
                }

                if (message.Text.ToLowerInvariant() == "help")
                {
                    this.ClearActiveTopic();

                    this.ShowHelp(context);
                    return Task.CompletedTask;
                }

                // If there is an active topic, let it handle this turn until it completes.
                if (HasActiveTopic)
                {
                    ActiveTopic.OnTurn(context);
                    return Task.CompletedTask;
                }

                ShowDefaultMessage(context);
            }

            return Task.CompletedTask;
        }

        private void ShowHelp(ITurnContext context)
        {
            var message = "Here's what I can do:\n\n";
            message += "To see your alarms, say 'Show Reservations'.\n\n";
            message += "To add an alarm, say 'Add Reservations'.\n\n";
            message += "To delete an alarm, say 'Delete Reservations'.\n\n";
            message += "To see this again, say 'Help'.";

            context.SendActivity(message);
        }
    }
}