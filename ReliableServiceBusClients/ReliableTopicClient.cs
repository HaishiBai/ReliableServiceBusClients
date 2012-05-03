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
    public class ReliableTopicClient: ReliableClientBase
    {
        private TopicClient mTopicClient;

        public ReliableTopicClient(string sbNamespace, TokenProvider tokenProvider, string path)
            : this(sbNamespace, tokenProvider, path, new RetryPolicy<ServiceBusTransientErrorDetectionStrategy>(new FixedInterval(3, TimeSpan.FromMilliseconds(100))))
        {
        }
        public ReliableTopicClient(string sbNamespace, TokenProvider tokenProvider, string path, RetryPolicy<ServiceBusTransientErrorDetectionStrategy> policy)
            : base(sbNamespace, tokenProvider, path, policy)
        {
            //create the queue if it doesn't exist
            bool needsCreation = false;
            try
            {
                needsCreation = !mRetryPolicy.ExecuteAction<bool>(() => mNamespaceManager.TopicExists(path));
            }
            catch (MessagingEntityNotFoundException)
            {
                needsCreation = true;
            }
            if (needsCreation)
            {
                try
                {
                    mRetryPolicy.ExecuteAction<TopicDescription>(() => mNamespaceManager.CreateTopic(path));
                }
                catch (MessagingEntityAlreadyExistsException)
                {
                    //ignore this exception because queue already exists
                }
            }
            mRetryPolicy.ExecuteAction(() => mTopicClient = mMessagingFactory.CreateTopicClient(path));   
        }
        public void Send(object message)
        {
            mRetryPolicy.ExecuteAction(() => mTopicClient.Send(new BrokeredMessage(message)));
        }
        public IAsyncResult BeginSend(object message, AsyncCallback callback, object state)
        {
            return mRetryPolicy.ExecuteAction<IAsyncResult>(() => mTopicClient.BeginSend(new BrokeredMessage(message), callback, state));
        }
        public void EndSend(IAsyncResult result)
        {
            mRetryPolicy.ExecuteAction(() => mTopicClient.EndSend(result));
        }
    }
}
