using System;
using System.Threading.Tasks;
using ReservationBot.Extensions;
using Microsoft.Bot.Builder;
using PromptlyBot;
using PromptlyBot.Validator;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Ai;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Samples
{
    public class AddReservationTopicState : ConversationTopicState
    {
        public Reservation reservation = new Reservation();
    }

    public class AddReservationTopic : ConversationTopic<AddReservationTopicState, Reservation>
    {
        private const string STARTDATE_PROMPT = "startdatePrompt";
        private const string LOCATION_PROMPT = "locationPrompt";
        private const string PEOPLENUMBER_PROMPT = "peoplenumberPrompt";
        private const string DURATION_PROMPT = "durationPrompt";
        private const string CONFIRMATION_PROMPT = "confirmationPrompt";
        private const string EDIT_PROMPT = "editPrompt";

        public AddReservationTopic() : base()
        {
            this.SubTopics.Add(STARTDATE_PROMPT, (object[] args) =>
            {
                var startdatePrompt = new Prompt<DateTime>();

                startdatePrompt.Set
                    .OnPrompt((context, lastTurnReason) =>
                    {
                        if (lastTurnReason != null && lastTurnReason == "novaliddatetime")
                        {
                            context.Reply("insert a valid date")
                                .Reply("Let's try again!");
                        }

                        context.Reply("What's your check in date?");
                    })
                    .Validator(new ReservationStartDateValidator())
                    .MaxTurns(2)
                    .OnSuccess((context, value) =>
                    {
                        this.ClearActiveTopic();
                        this.State.reservation.StartDay = value;
                        this.OnReceiveActivity(context);
                    })
                    .OnFailure((context, reason) =>
                    {
                        this.ClearActiveTopic();
                        if (reason != null && reason == "toomanyattemps")
                        {
                            context.Reply("I'm sorry I'm having issues understanding you.");
                        }
                        this.OnFailure(context, reason);
                    });
                return startdatePrompt;
            });

            this.SubTopics.Add(LOCATION_PROMPT, (object[] args) =>
            {
                var locationPrompt = new Prompt<string>();

                locationPrompt.Set
                    .OnPrompt((context, lastTurnReason) =>
                    {
                        context.Reply("Where you want to go?");
                    })
                    .Validator(new ReservationLocationValidator())
                    .MaxTurns(2)
                    .OnSuccess((context, value) =>
                    {
                        this.ClearActiveTopic();
                        this.State.reservation.Location = value;
                        this.OnReceiveActivity(context);
                    })
                    .OnFailure((context, reason) =>
                    {
                        this.ClearActiveTopic();
                        if (reason != null && reason == "toomanyattemps")
                        {
                            context.Reply("I'm sorry I'm having issues understanding you.");
                        }
                        this.OnFailure(context, reason);
                    });
                return locationPrompt;
            });

            this.SubTopics.Add(PEOPLENUMBER_PROMPT, (object[] args) =>
            {
                var peoplenumberPrompt = new Prompt<int>();

                peoplenumberPrompt.Set
                    .OnPrompt((context, lastTurnReason) =>
                    {
                        context.Reply("How many people?");
                    })
                    .Validator(new ReservationNumberValidator())
                    .MaxTurns(2)
                    .OnSuccess((context, value) =>
                    {
                        this.ClearActiveTopic();
                        this.State.reservation.PeopleNumber = value;
                        this.OnReceiveActivity(context);
                    })
                    .OnFailure((context, reason) =>
                    {
                        this.ClearActiveTopic();
                        if (reason != null && reason == "toomanyattemps")
                        {
                            context.Reply("I'm sorry I'm having issues understanding you.");
                        }
                        this.OnFailure(context, reason);
                    });
                return peoplenumberPrompt;
            });

            this.SubTopics.Add(DURATION_PROMPT, (object[] args) =>
            {
                var durationPrompt = new Prompt<int>();

                durationPrompt.Set
                    .OnPrompt((context, lastTurnReason) =>
                    {
                        context.Reply("How many days do you want to stay?");
                    })
                    .Validator(new ReservationNumberValidator())
                    .MaxTurns(2)
                    .OnSuccess((context, value) =>
                    {
                        this.ClearActiveTopic();
                        this.State.reservation.Duration = value;
                        this.OnReceiveActivity(context);
                    })
                    .OnFailure((context, reason) =>
                    {
                        this.ClearActiveTopic();
                        if (reason != null && reason == "toomanyattemps")
                        {
                            context.Reply("I'm sorry I'm having issues understanding you.");
                        }
                        this.OnFailure(context, reason);
                    });
                return durationPrompt;
            });

            this.SubTopics.Add(CONFIRMATION_PROMPT, (object[] args) =>
            {
                var confirmationPrompt = new ReservationConfirmationTopic(this.State.reservation);

                confirmationPrompt.Set
               .OnSuccess((ctx, confirmation) =>
               {
                   if (confirmation.confirmationState == "yes")
                   {
                       this.ClearActiveTopic();
                       this.State.reservation.Confirmed = "yes";
                       this.OnReceiveActivity(ctx);
                   }
                   else
                   {
                       this.ClearActiveTopic();
                       this.State.reservation.Confirmed = "no";
                       this.OnReceiveActivity(ctx);
                   }

               })
               .OnFailure((context, reason) =>
               {
                   this.ClearActiveTopic();
                   if (reason != null && reason == "toomanyattemps")
                   {
                       context.Reply("I'm sorry I'm having issues understanding you.");
                   }
                   this.OnFailure(context, reason);
               });

                return confirmationPrompt;
            });

            this.SubTopics.Add(EDIT_PROMPT, (object[] args) =>
            {
                var editPrompt = new Prompt<string>();

                editPrompt.Set
                    .OnPrompt((context, lastTurnReason) =>
                    {
                        var actions = new Dictionary<string, string>();
                        actions.Add("startdate", "Change Start Date");
                        actions.Add("location", "Change Location");
                        actions.Add("duration", "Change Duration");
                        actions.Add("peoplenumber", "Change People Number");
                        actions.Add("confirmreservation", "Confirm Reservation");

                       var activity = ReservationView.CreateEditReservationHeroCard(actions);
                        // Send the activity as a reply to the user.
                        context.Reply(activity);
                    })
                    .Validator(new ReservationEditValidator())
                    .MaxTurns(2)
                    .OnSuccess((context, value) =>
                    {
                        this.ClearActiveTopic();

                        switch (value)
                        {
                            case "location":
                                this.State.reservation.Location = null;
                                break;
                            case "startdate":
                                this.State.reservation.StartDay = default;
                                break;
                            case "duration":
                                this.State.reservation.Duration = 0;
                                break;
                            case "peoplenumber":
                                this.State.reservation.PeopleNumber = 0;
                                break;
                            case "confirmreservation":
                                this.State.reservation.Confirmed = "yes";
                                break;
                            default:
                                break;
                        }
                        this.OnReceiveActivity(context);
                    })
                    .OnFailure((context, reason) =>
                    {
                        this.ClearActiveTopic();
                        if (reason != null && reason == "toomanyattemps")
                        {
                            context.Reply("I'm sorry I'm having issues understanding you.");
                        }
                        this.OnFailure(context, reason);
                    });
                return editPrompt;
            });
        }

        public override Task OnReceiveActivity(IBotContext context)
        {
            if (HasActiveTopic)
            {
                ActiveTopic.OnReceiveActivity(context);
                return Task.CompletedTask;
            }

            if (this.State.reservation.StartDay == default)
            {
                this.SetActiveTopic(STARTDATE_PROMPT);
                this.ActiveTopic.OnReceiveActivity(context);
                return Task.CompletedTask;
            }

            if (this.State.reservation.Location == null)
            {
                this.SetActiveTopic(LOCATION_PROMPT);
                this.ActiveTopic.OnReceiveActivity(context);
                return Task.CompletedTask;
            }

            if (this.State.reservation.PeopleNumber == 0)
            {
                this.SetActiveTopic(PEOPLENUMBER_PROMPT);
                this.ActiveTopic.OnReceiveActivity(context);
                return Task.CompletedTask;
            }
            if (this.State.reservation.Duration == 0)
            {
                this.SetActiveTopic(DURATION_PROMPT);
                this.ActiveTopic.OnReceiveActivity(context);
                return Task.CompletedTask;
            }
            if (this.State.reservation.Confirmed == null)
            {
                this.SetActiveTopic(CONFIRMATION_PROMPT);
                this.ActiveTopic.OnReceiveActivity(context);
                return Task.CompletedTask;
            }
            else
            {
                if (State.reservation.Confirmed == "no")
                {
                    this.SetActiveTopic(EDIT_PROMPT);
                    this.ActiveTopic.OnReceiveActivity(context);
                    return Task.CompletedTask;
                }
            }

            this.OnSuccess(context, this.State.reservation);

            return Task.CompletedTask;
        }
    }

    internal class ReservationEditValidator : Validator<string>
    {
        public override ValidatorResult<string> Validate(IBotContext context)
        {
            return new ValidatorResult<string>
            {
                Value = context.Request.AsMessageActivity().Text
            };
        }
    }

    internal class ReservationConfirmationValidator : Validator<string>
    {
        public override ValidatorResult<string> Validate(IBotContext context)
        {
            return new ValidatorResult<string>
            {
                Value = context.Request.AsMessageActivity().Text
            };
        }
    }

    internal class ReservationNumberValidator : Validator<int>
    {
        public override ValidatorResult<int> Validate(IBotContext context)
        {
            return new ValidatorResult<int>
            {
                Value = Int32.Parse(context.Request.AsMessageActivity().Text)
            };
        }
    }

    internal class ReservationLocationValidator : Validator<string>
    {
        public override ValidatorResult<string> Validate(IBotContext context)
        {
            return new ValidatorResult<string>
            {
                Value = context.Request.AsMessageActivity().Text
            };
        }
    }

    public class ReservationStartDateValidator : Validator<DateTime>
    {
        public override ValidatorResult<DateTime> Validate(IBotContext context)
        {
            var reco = DateTimeRecognizerExtension.GetDateTimes(context);
            if (reco.isValid)
            {
                return new ValidatorResult<DateTime>
                {
                    Value = (DateTime)reco.dt
                };
            }
            else
            {
                return new ValidatorResult<DateTime>
                {
                    Reason = "novaliddatetime"
                };
            }

        }
    }
}
