using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceBus;
using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.ServiceBus;
using Microsoft.Practices.TransientFaultHandling;

namespace ReliableServiceBusClients
{
    public class ReliableQueueClient : ReliableClientBase, IDisposable
    {
        private QueueClient mQueueClient;
        public ReliableQueueClient(string sbNamespace, TokenProvider tokenProvider, string path, ReceiveMode receiveMode, RetryPolicy<ServiceBusTransientErrorDetectionStrategy> policy):base(sbNamespace, tokenProvider,path,policy)
        {
            //create the queue if it doesn't exist
            bool needsCreation = false;
            try
            {
                needsCreation = !mRetryPolicy.ExecuteAction<bool>(() => mNamespaceManager.QueueExists(path));
            }
            catch (MessagingEntityNotFoundException)
            {
                needsCreation = true;
            }
            if (needsCreation)
            {
                try
                {
                    mRetryPolicy.ExecuteAction<QueueDescription>(() => mNamespaceManager.CreateQueue(path));
                }
                catch (MessagingEntityAlreadyExistsException)
                {
                    //ignore this exception because queue already exists
                }
            }
            mRetryPolicy.ExecuteAction(() => mQueueClient = mMessagingFactory.CreateQueueClient(path, receiveMode));   
        }
        public ReliableQueueClient(string sbNamespace, TokenProvider tokenProvider, string path, RetryPolicy<ServiceBusTransientErrorDetectionStrategy> policy)
            : this(sbNamespace, tokenProvider, path, ReceiveMode.PeekLock, policy)
        {
        }
        public ReliableQueueClient(string sbNamespace, TokenProvider tokenProvider, string path, ReceiveMode receiveMode) :
            this(sbNamespace, tokenProvider, path, receiveMode,
             new RetryPolicy<ServiceBusTransientErrorDetectionStrategy>(new FixedInterval(3, TimeSpan.FromMilliseconds(100))))
        {
        }
        public ReliableQueueClient(string sbNamespace, TokenProvider tokenProvider, string path)
            : this(sbNamespace, tokenProvider, path, ReceiveMode.PeekLock)
        {
        }
        public void Send(object message)
        {
            mRetryPolicy.ExecuteAction(() => mQueueClient.Send(new BrokeredMessage(message)));
        }
        public IAsyncResult BeginSend(object message, AsyncCallback callback, object state)
        {
            return mRetryPolicy.ExecuteAction<IAsyncResult>(() => mQueueClient.BeginSend(new BrokeredMessage(message), callback, state));
        }
        public void EndSend(IAsyncResult result)
        {
            mRetryPolicy.ExecuteAction(() => mQueueClient.EndSend(result));
        }
        public ReliableBrokeredMessage Receive()
        {
            return mRetryPolicy.ExecuteAction<ReliableBrokeredMessage>(() => { return new ReliableBrokeredMessage(mRetryPolicy, mQueueClient.Receive()); });
        }

        public void Dispose()
        {   
            mQueueClient.Close();
            mMessagingFactory.Close();
        }
    }
}
