using System.Collections.Generic;
using System.Threading.Tasks;

#if USING_QUEUES
using Azure.Storage.Queues.Models;
#endif

namespace Infrastructure.Interfaces
{
    public interface IQueueService
    {
        string QueueName { get; set; }
        bool CreateIfNotExists { get; set; }
        
#if USING_QUEUES

        Task<PeekedMessage> PeekMessageAsync(string queueName = "");
        Task<IEnumerable<PeekedMessage>> PeekMessagesAsync(string queueName = "", int? maxMessages = default);
        Task<QueueMessage> ReceiveMessageAsync(string queueName = "");
        Task<IEnumerable<QueueMessage>> ReceiveMessagesAsync(string queueName = "", int? maxMessages = default);
        Task<SendReceipt> SendMessageAsync(string queueName = "", string content = "");
#else
        Task<T> PeekMessageAsync<T>(string queueName = "") where T : class;
        Task<IEnumerable<T>> PeekMessagesAsync<T>(string queueName = "") where T : class;
        Task<T> ReceiveMessageAsync<T>(string queueName = "") where T : class;
        Task<IEnumerable<T>> ReceiveMessagesAsync<T>(string queueName = "") where T : class;
        Task<T> SendMessageAsync<T>(string queueName = "", string content = "") where T : class;
#endif
    }
}
