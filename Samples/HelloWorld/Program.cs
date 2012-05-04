using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReliableServiceBusClients;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceBus;
using System.Threading;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            string owner = "owner";
            string secretkey = "{YOUR KEY}";
            string nameSpace = "hbai01";

            using (ReliableQueueClient client = new ReliableQueueClient(nameSpace,
                                                                        TokenProvider.CreateSharedSecretTokenProvider(owner, secretkey),
                                                                        "testqueue"))
            {
                int messageCount = 10;

                Console.WriteLine("Synchrouous send:");
                for (int i = 1; i <= messageCount; i++)
                {
                    client.Send(string.Format("Message {0} to myself", i));

                    var message = client.Receive();
                    if (message != null)
                    {
                        Console.WriteLine("Got message: " + message.Original.GetBody<string>());
                        message.Complete();
                    }
                }

                Console.Write("Press [Enter] to test asynchrouous send:");
                Console.ReadLine();

                int count = 0;
                AutoResetEvent evt = new AutoResetEvent(false);
                for (int i = 1; i <= messageCount; i++)
                {
                    client.BeginSend(string.Format("Async Message {0} to myself", i),
                        (res) =>
                        {
                            try
                            {
                                client.EndSend(res);
                                var message = client.Receive();
                                if (message != null)
                                {
                                    Console.WriteLine("Got message: " + message.Original.GetBody<string>());
                                    message.Complete();
                                }
                            }
                            finally
                            {
                                Interlocked.Increment(ref count);
                                if (Interlocked.Equals(count, 9))
                                    evt.Set();
                            }
                        }, null);
                }
                evt.WaitOne();
            }

            Console.Write("Press [Enter] to test pub/sub:");
            Console.ReadLine();

            ReliableTopicClient topicClient = new ReliableTopicClient(nameSpace,
                                                                        TokenProvider.CreateSharedSecretTokenProvider(owner, secretkey),
                                                                        "testtopic");
            ReliableSubscriptionClient subscriptionClient = new ReliableSubscriptionClient(nameSpace,
                                                                        TokenProvider.CreateSharedSecretTokenProvider(owner, secretkey),
                                                                        "testtopic", "testsubscription");
            topicClient.Send("Broadcasting message");
            Console.WriteLine("Received: " + subscriptionClient.Receive().Original.GetBody<string>());

            Console.Write("Press [Enter] to exit:");
            Console.ReadLine();
        }
    }
}
