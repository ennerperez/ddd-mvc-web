using System.Collections.Generic;
using System.Threading.Tasks;

#if USING_QUEUES
using Azure.Storage.Queues.Models;
#endif

namespace Infrastructure.Interfaces
{
    public interface IQueueService
    {
#if USING_QUEUES
        Task<IEnumerable<PeekedMessage>> GetMessagesAsync();

        Task<QueueMessage> ReceiveMessageAsync();

        Task<SendReceipt> SendMessageAsync(string content);
#else
        Task<IEnumerable<T>> GetMessagesAsync<T>() where T : class;

        Task<T> ReceiveMessageAsync<T>() where T : class;

        Task<T> SendMessageAsync<T>(string content) where T : class;
#endif
    }
}
