using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReliableServiceBusClients;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceBus;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            //Use ReliableQueueClient
            using (ReliableQueueClient client = new ReliableQueueClient("{YOUR SB NAMESPACE}", 
                                                                        TokenProvider.CreateSharedSecretTokenProvider("{YOUR SECRET", "{YOUR SECRET KEY}"),
                                                                        "testqueue"))
            {
                Console.WriteLine("Synchrouous send:");
                for (int i = 1; i <= 10; i++)
                {
                    client.Send(string.Format("Message {0} to myself", i));

                    var message = client.Receive();
                    if (message != null)
                    {
                        Console.WriteLine("Got message: " + message.Original.GetBody<string>());
                        message.Complete();
                    }
                }

                Console.WriteLine("Press [Enter] to test asynchrouous send:");
                Console.ReadLine();

                for (int i = 1; i <= 10; i++)
                {
                    client.BeginSend(string.Format("Async Message {0} to myself", i),
                        (res) =>
                        {
                            client.EndSend(res);
                                var message = client.Receive();
                                if (message != null)
                                {
                                    Console.WriteLine("Got message: " + message.Original.GetBody<string>());
                                    message.Complete();
                                }
                        }, null);
                }

                Console.ReadLine();
            }
        }
    }
}
