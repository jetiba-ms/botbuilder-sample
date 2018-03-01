using System;

namespace Microsoft.Bot.Samples
{
    public class Reservation
    {
        public DateTime StartDay { get; set; }
        public string Location { get; set; }
        public int PeopleNumber { get; set; }
        public int Duration { get; set; }
        public string Confirmed { get; set; }
    }
}