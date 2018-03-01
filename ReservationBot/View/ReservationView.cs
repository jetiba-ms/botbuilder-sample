using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Bot.Samples
{
    public class ReservationView
    {
        public static void ShowReservations(IBotContext context, List<Reservation> reservations)
        {
            if ((reservations == null) || (reservations.Count == 0))
            {
                context.Reply("You have no reservations.");
                return;
            }

            IMessageActivity activity;
            if (reservations.Count == 1)
            {
                activity = ((Activity)context.Request).CreateReply();
                var card = new AdaptiveCard();
                var factset = new AdaptiveFactSet();
                factset.Facts.Add(new AdaptiveFact("Start Date", reservations[0].StartDay.ToString("dd/MM/yyyy")));
                factset.Facts.Add(new AdaptiveFact("Location", reservations[0].Location));
                factset.Facts.Add(new AdaptiveFact("Duration", reservations[0].Duration.ToString()));
                factset.Facts.Add(new AdaptiveFact("People", reservations[0].PeopleNumber.ToString()));

                card.Body.Add(new AdaptiveTextBlock() { Text = "Your reservation", Size = AdaptiveTextSize.Default, Wrap = true, Weight = AdaptiveTextWeight.Default });
                card.Body.Add(factset);

                activity.Attachments.Add(new Attachment(AdaptiveCard.ContentType, content: card));
                context.Reply(activity);
                return;
            }

            string message = $"You have { reservations.Count } reservations: \n\n";
            var attachments = new List<Attachment>();
           
            foreach (var res in reservations)
            {
                
                var card = new AdaptiveCard();
                var factset = new AdaptiveFactSet();
                factset.Facts.Add(new AdaptiveFact("Start Date", res.StartDay.ToString("dd/MM/yyyy")));
                factset.Facts.Add(new AdaptiveFact("Location", res.Location));
                factset.Facts.Add(new AdaptiveFact("Duration", res.Duration.ToString()));
                factset.Facts.Add(new AdaptiveFact("People", res.PeopleNumber.ToString()));

                card.Body.Add(new AdaptiveTextBlock() { Text = "Your reservation", Size = AdaptiveTextSize.Default, Wrap = true, Weight = AdaptiveTextWeight.Default });
                card.Body.Add(factset);

                attachments.Add(new Attachment(AdaptiveCard.ContentType, content: card));
            }
             activity = MessageFactory.Carousel(attachments);

            context.Reply(activity);
        }

        public static IMessageActivity ReservationRecapCard(IBotContext context, Reservation reservation)
        {
            IMessageActivity activity = ((Activity)context.Request).CreateReply();
            var card = new AdaptiveCard();
            var factset = new AdaptiveFactSet();
            factset.Facts.Add(new AdaptiveFact("Start Date", reservation.StartDay.ToString("dd/MM/yyyy")));
            factset.Facts.Add(new AdaptiveFact("Location", reservation.Location));
            factset.Facts.Add(new AdaptiveFact("Duration", reservation.Duration.ToString()));
            factset.Facts.Add(new AdaptiveFact("People", reservation.PeopleNumber.ToString()));
            
            card.Body.Add(new AdaptiveTextBlock() { Text = "Your reservation", Size = AdaptiveTextSize.Default, Wrap = true, Weight = AdaptiveTextWeight.Default });
            card.Body.Add(factset);

            activity.Attachments.Add(new Attachment(AdaptiveCard.ContentType, content: card));

            return activity;
                
        }

        public static IMessageActivity CreatedYesNoCard()
        {
            var activity = MessageFactory.Attachment(
                new HeroCard(
                   title: "Do you want to confirm the reservation?",
                   buttons: new CardAction[]
                   {
                       new CardAction(
                           type: "imBack",
                           title: "Yes",
                           value: "yes"
                           ),
                       new CardAction(
                            type: "imBack",
                           title: "No",
                           value: "no"
                           )
                   }
                ).ToAttachment());
            return activity;
        }

    public static IMessageActivity CreateEditReservationHeroCard(Dictionary<string, string> actions)
    {
        // Create the activity and attach a Hero card to show possibilities to change fields
        var buttons = new List<CardAction>();
        foreach (var a in actions)
        {
            var button = new CardAction(
                type: "imBack",
                title: a.Value,
                text: a.Key,
                displayText: "Change " + a.Value,
                value: a.Key
                );
            buttons.Add(button);
        }

        var activity = MessageFactory.Attachment(
            new HeroCard(
                title: "Do you want to change reservation information?",
                buttons: buttons
                )
            .ToAttachment());
        return activity;
    }
}
}