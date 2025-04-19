using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#if USING_QUEUES
using Azure;
using Azure.Storage.Queues.Models;
#endif

namespace Infrastructure.Interfaces
{
    public interface IQueueService
    {
        string QueueName { get; set; }
        bool CreateIfNotExists { get; set; }

#if USING_QUEUES
        Task<PeekedMessage> PeekMessageAsync(string queueName = "", CancellationToken cancellationToken = default);
        Task<IEnumerable<PeekedMessage>> PeekMessagesAsync(string queueName = "", int? maxMessages = null, CancellationToken cancellationToken = default);
        Task<QueueMessage> ReceiveMessageAsync(string queueName = "", TimeSpan? visibilityTimeout = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<QueueMessage>> ReceiveMessagesAsync(string queueName = "", int? maxMessages = null, TimeSpan? visibilityTimeout = null, CancellationToken cancellationToken = default);
        Task<SendReceipt> SendMessageAsync(string queueName = "", string content = "", CancellationToken cancellationToken = default);
        Task<Response> DeleteMessageAsync(QueueMessage message, string queueName = "", CancellationToken cancellationToken = default);
#else
        Task<T> PeekMessageAsync<T>(string queueName = "", CancellationToken cancellationToken = default) where T : class;
        Task<IEnumerable<T>> PeekMessagesAsync<T>(string queueName = "", CancellationToken cancellationToken = default) where T : class;
        Task<T> ReceiveMessageAsync<T>(string queueName = "", TimeSpan? visibilityTimeout = null, CancellationToken cancellationToken = default) where T : class;
        Task<IEnumerable<T>> ReceiveMessagesAsync<T>(string queueName = "", int? maxMessages = null, TimeSpan? visibilityTimeout = null, CancellationToken cancellationToken = default) where T : class;
        Task<T> SendMessageAsync<T>(string queueName = "", string content = "", CancellationToken cancellationToken = default) where T : class;
        Task<TResponse> DeleteMessageAsync<TResponse, TMessage>(TMessage message, string queueName = "", CancellationToken cancellationToken = default) where TResponse : class where TMessage : class;
#endif
    }
}
