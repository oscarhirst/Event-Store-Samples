using System;
using System.Net;
using System.Text;
using System.Timers;
using EventStore.ClientAPI;//3.6.0

namespace ESWriteApp
{
    class Program
    {
        public static IEventStoreConnection connection;
        public static int intervalCount;

        static void Main()
        {
            connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113));

            var timer = new Timer(1000);
            timer.Interval = 1000;
            timer.Elapsed += SendEvent;
            timer.AutoReset = true;

            connection.ConnectAsync().Wait();

            timer.Start();

            while (intervalCount < 1000)
            {
            }
            Console.WriteLine("Completed");
        }

        static void SendEvent(object source, ElapsedEventArgs e)
        {
            var currentTimeEvent = new EventData(Guid.NewGuid(), "testEvent", false,
                                        Encoding.UTF8.GetBytes(DateTime.Now.ToString()),
                                        Encoding.UTF8.GetBytes($"{intervalCount} From Oscar"));

            connection.AppendToStreamAsync("test-stream", ExpectedVersion.Any, currentTimeEvent).Wait();

            Console.WriteLine($"Wrote {intervalCount} @ {DateTime.Now.ToString()}");

            intervalCount++;
        }
    }
}
