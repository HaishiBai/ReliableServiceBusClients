using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus;
using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.ServiceBus;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.ServiceBus.Messaging;

namespace ReliableServiceBusClients
{
    public class ReliableSubscriptionClient : ReliableClientBase
    {
        private SubscriptionClient mSubscriptionClient;

        public ReliableSubscriptionClient(string sbNamespace, TokenProvider tokenProvider, string path, string name)
            : this(sbNamespace, tokenProvider, path, name, new RetryPolicy<ServiceBusTransientErrorDetectionStrategy>(new FixedInterval(3, TimeSpan.FromMilliseconds(100))))
        {
        } 
        public ReliableSubscriptionClient(string sbNamespace, TokenProvider tokenProvider, string path, string name, RetryPolicy<ServiceBusTransientErrorDetectionStrategy> policy)
            : base(sbNamespace, tokenProvider, path, policy)
        {
            //create the queue if it doesn't exist
            bool needsCreation = false;
            try
            {
                needsCreation = !mRetryPolicy.ExecuteAction<bool>(() => mNamespaceManager.SubscriptionExists(path,name));
            }
            catch (MessagingEntityNotFoundException)
            {
                needsCreation = true;
            }
            if (needsCreation)
            {
                try
                {
                    mRetryPolicy.ExecuteAction<SubscriptionDescription>(() => mNamespaceManager.CreateSubscription(path,name));
                }
                catch (MessagingEntityAlreadyExistsException)
                {
                    //ignore this exception because queue already exists
                }
            }
            mRetryPolicy.ExecuteAction(() => mSubscriptionClient = mMessagingFactory.CreateSubscriptionClient(path,name));   
        }
        public ReliableBrokeredMessage Receive()
        {
            return mRetryPolicy.ExecuteAction<ReliableBrokeredMessage>(() => { return new ReliableBrokeredMessage(mRetryPolicy, mSubscriptionClient.Receive()); });
        }
    }
}
