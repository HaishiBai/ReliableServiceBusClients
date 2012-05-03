using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.ServiceBus;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.Practices.TransientFaultHandling;

namespace ReliableServiceBusClients
{
    public abstract class ReliableClientBase
    {
        protected RetryPolicy<ServiceBusTransientErrorDetectionStrategy> mRetryPolicy;
        protected NamespaceManager mNamespaceManager;
        protected MessagingFactory mMessagingFactory;

        public ReliableClientBase(string sbNamespace, TokenProvider tokenProvider, string path, RetryPolicy<ServiceBusTransientErrorDetectionStrategy> policy)
        {
            mRetryPolicy = policy;
            Uri address = ServiceBusEnvironment.CreateServiceUri("sb", sbNamespace, string.Empty);
            mNamespaceManager = new NamespaceManager(address, tokenProvider);
            mMessagingFactory = MessagingFactory.Create(address, tokenProvider);
        }
    }
}
