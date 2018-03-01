using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using PromptlyBot;

namespace Microsoft.Bot.Samples
{
    internal class RootTopic : TopicsRoot
    {
        private const string ADD_RESERVATION_TOPIC = "addReservationTopic";
        private const string DELETE_RESERVATION_TOPIC = "deleteReservationTopic";

        private const string USER_STATE_RESERVATION = "Reservations";

        public RootTopic(IBotContext context) : base(context)
        {
            if (context.State.UserProperties[USER_STATE_RESERVATION] == null)
            {
                context.State.UserProperties[USER_STATE_RESERVATION] = new List<Reservation>();
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
                        ((List<Reservation>)ctx.State.UserProperties[USER_STATE_RESERVATION]).Add(reservation);
                        context.Reply($"Reservation added!");
                    }
                    else
                    {
                        context.Reply("you deleted the reservation");
                    }
                    
                })
                .OnFailure((ctx, reason) =>
                {
                    this.ClearActiveTopic();
                    context.Reply("It fails for some reasons");
                    this.ShowDefaultMessage(context);
                });

                return addReservationTopic;

            });               
        }

        private void ShowDefaultMessage(IBotContext context)
        {
            context.Reply("write Reservation to start with a new reservation");
        }

        public override Task OnReceiveActivity(IBotContext context)
        {
            if ((context.Request.Type == ActivityTypes.Message) && (context.Request.AsMessageActivity().Text.Length > 0))
            {
                var message = context.Request.AsMessageActivity();
                
                //I can use LUIS here!

                // If the user wants to change the topic of conversation...
                if (message.Text.ToLowerInvariant() == "add reservation")
                {
                    // Set the active topic and let the active topic handle this turn.
                    this.SetActiveTopic(ADD_RESERVATION_TOPIC)
                            .OnReceiveActivity(context);
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

                    ReservationView.ShowReservations(context, context.State.UserProperties[USER_STATE_RESERVATION]);
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
                    ActiveTopic.OnReceiveActivity(context);
                    return Task.CompletedTask;
                }

                ShowDefaultMessage(context);
            }

            return Task.CompletedTask;
        }

        private void ShowHelp(IBotContext context)
        {
            var message = "Here's what I can do:\n\n";
            message += "To see your alarms, say 'Show Reservations'.\n\n";
            message += "To add an alarm, say 'Add Reservations'.\n\n";
            message += "To delete an alarm, say 'Delete Reservations'.\n\n";
            message += "To see this again, say 'Help'.";

            context.Reply(message);
        }
    }
}