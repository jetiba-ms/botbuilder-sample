using Microsoft.Bot.Builder;
using Microsoft.Recognizers.Text.DateTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReservationBot.Extensions
{
    public class TimeValues
    {
        public bool isValid { get; set; }
        public DateTime? dt { get; set; }
        public int duration { get; set; }

    }

    public static class DateTimeRecognizerExtension
    {
        public static TimeValues GetDateTimes(this IBotContext context)
        {
            //IList<DateTime> times = new List<DateTime>();
            // Get DateTime model for English
            var model = DateTimeRecognizer.GetInstance().GetDateTimeModel(context.Request.AsMessageActivity().Locale ?? "en-us");
            var results = model.Parse(context.Request.AsMessageActivity().Text);

            // Check there are valid results
            if (results.Any() && results.First().TypeName.StartsWith("datetimeV2"))
            {
                // The DateTime model can return several resolution types (https://github.com/Microsoft/Recognizers-Text/blob/master/.NET/Microsoft.Recognizers.Text.DateTime/Constants.cs#L7-L14)
                // We only care for those with a date, date and time, or date time period:
                // date, daterange, datetime, datetimerange

                var first = results.First();
                var resolutionValues = (IList<Dictionary<string, string>>)first.Resolution["values"];

                var subType = first.TypeName.Split('.').Last();
                if (subType.Contains("date") && !subType.Contains("range"))
                {
                    // a date (or date & time) or multiple
                    var moment = resolutionValues.Select(v => DateTime.Parse(v["value"]))
                        .Where(x => x.Year >= DateTime.Now.Year)
                        .FirstOrDefault();

                    //today
                    var tomorrow = DateTime.Now.AddDays(1);
                    if (moment >= tomorrow)
                    {
                        return new TimeValues
                        {
                            isValid = true,
                            dt = moment.Date,
                            duration = 0
                        };
                    }
                    else
                    {
                        return new TimeValues
                        {
                            isValid = false,
                            dt = null,
                            duration = 0
                        };
                    }
                }
                else if (subType.Contains("date") && subType.Contains("range"))
                {
                    // range
                    var from = DateTime.Parse(resolutionValues.First()["start"]);
                    var to = DateTime.Parse(resolutionValues.First()["end"]);

                    if (to > from)
                    {
                        var tomorrow = DateTime.Now.AddDays(1);
                        if (from >= tomorrow && to >= tomorrow)
                        {
                            return new TimeValues
                            {
                                isValid = true,
                                dt = from.Date,
                                duration = to.Day - from.Day
                            };
                        }
                        else
                        {
                            return new TimeValues
                            {
                                isValid = false,
                                dt = null,
                                duration = 0
                            };
                        }
                    }
                    else
                    {
                        return new TimeValues
                        {
                            isValid = false,
                            dt = null,
                            duration = 0
                        };
                    }
                }
            }
            else
                return new TimeValues
                {
                    isValid = false,
                    dt = null,
                    duration = 0
                };

            return new TimeValues
            {
                isValid = false,
                dt = null,
                duration = 0
            };
        }
    }
}
