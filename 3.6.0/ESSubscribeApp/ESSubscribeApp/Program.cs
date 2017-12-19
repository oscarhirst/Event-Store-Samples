using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using EventStore.ClientAPI;//3.6.0

namespace ESSubscribeApp
{
    class Program
    {
        static void Main()
        {
            const string stream = "test-stream";

            var eventDataDictionary = new Dictionary<int, string>();
            var events = 0;

            Action<EventStoreCatchUpSubscription, ResolvedEvent> eventAction =
                delegate (EventStoreCatchUpSubscription s, ResolvedEvent e)
                {
                    var number = e.Event.EventNumber;
                    var data = Encoding.UTF8.GetString(e.Event.Data);
                    eventDataDictionary.Add(number, data);
                    events++;
                    Console.WriteLine($"Event {events}: {eventDataDictionary[events]}");
                };

            Action<EventStoreCatchUpSubscription, SubscriptionDropReason, Exception> subscriptionDropAction =
                delegate (EventStoreCatchUpSubscription s, SubscriptionDropReason r, Exception ex)
                {
                    Console.WriteLine($"An exception occured: {ex}");
                    Console.WriteLine($"{s} subscription dropped: {r}");
                    Console.WriteLine("Stored Events:");
                    foreach (var ele in eventDataDictionary)
                    {
                        Console.WriteLine($"Event {ele.Key}: {ele.Value}");
                    }
                };

            using (var connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113)))
            {
                connection.ConnectAsync().Wait();

                connection.SubscribeToStreamFrom (
                    stream, 
                    lastCheckpoint: StreamPosition.Start, 
                    resolveLinkTos: true, 
                    eventAppeared: eventAction, 
                    liveProcessingStarted: x => { Console.WriteLine("All caught up. Subscribing to the stream"); },
                    subscriptionDropped: subscriptionDropAction, userCredentials: null, readBatchSize: 100
                );

                Console.WriteLine("Waiting for events. Press enter to exit");
                Console.ReadLine();
            }
        }
    }
}
