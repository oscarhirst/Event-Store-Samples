using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using EventStore.ClientAPI;//4.0.3

namespace ESReadApp
{
    class Program
    {
        static void Main()
        {
            StreamEventsSlice streamEvents;

            using (var connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113)))
            {
                connection.ConnectAsync().Wait();

                streamEvents = connection.ReadStreamEventsForwardAsync(
                                            "test-stream", StreamPosition.Start, 2500, true).Result;
            }

            var returnedEvents = streamEvents.Events;

            var streamLength = returnedEvents.Length;

            var eventDataDictionary = new Dictionary<long, string>();

            var eventCount = 0;

            foreach (ResolvedEvent eve in returnedEvents)
            {
                var number = eve.Event.EventNumber;
                var data = Encoding.UTF8.GetString(eve.Event.Data);

                eventDataDictionary.Add(number, data);

                eventCount++;

                //For testing:
                //Console.WriteLine($"Caught up on {eventCount}/{streamLength}: {data}");

                if (eventCount == 0 || eventCount % 50 == 0)
                    Console.WriteLine($"Caught up on {eventCount}/{streamLength} events in stream");
            }
            Console.WriteLine("All caught up");
        }
    }
}

