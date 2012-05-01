using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.ServiceBus.Messaging;

namespace ReliableServiceBusClients
{
    public class ReliableBrokeredMessage
    {
        private RetryPolicy mRetryPolicy;
        public BrokeredMessage Original { get; private set; }
        public ReliableBrokeredMessage(RetryPolicy policy, BrokeredMessage message)
        {
            mRetryPolicy = policy;
            Original = message;
        }
        public void Complete()
        {
            mRetryPolicy.ExecuteAction(() => Original.Complete());
            if (Original != null)
            {
                Original.Dispose();
                Original = null;
            }
        }
    }
}
