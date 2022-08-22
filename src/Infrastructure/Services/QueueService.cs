#if USING_QUEUES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class QueueService : IQueueService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        private readonly List<QueueClient> _clients;

        public QueueService(IConfiguration configuration, ILoggerFactory logger)
        {
            _configuration = configuration;
            _logger = logger.CreateLogger(GetType());
            _clients = new List<QueueClient>();
        }

        public string QueueName { get; set; }
        public bool CreateIfNotExists { get; set; }

        private async Task<QueueClient> GetClientAsync(string queueName = "", CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(queueName)) queueName = QueueName;
            var client = _clients.FirstOrDefault(c => c.Name == queueName);
            if (client != null)
                return client;

            try
            {
                _logger.LogInformation("Initializing [{QueueName}] queue client", queueName);
                var connectionString = _configuration["AzureSettings:Storage:ConnectionString"];
                client = new QueueClient(connectionString, queueName);
                if (CreateIfNotExists)
                    await client.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
                else if (await client.ExistsAsync())
                    _clients.Add(client);
                else
                    client = null;
                return client;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Message}", e.Message);
                throw new TypeInitializationException(GetType().FullName, new Exception($"Could not initialize the [{queueName}] queue client.", e));
            }
        }

        public async Task<PeekedMessage> PeekMessageAsync(string queueName = "")
        {
            var client = await GetClientAsync(queueName);
            if (client == null) return null;
            var message = await client.PeekMessageAsync();
            if (message != null && message.Value != null)
                return message.Value;

            return null;
        }

        public async Task<IEnumerable<PeekedMessage>> PeekMessagesAsync(string queueName = "", int? maxMessages = default)
        {
            var client = await GetClientAsync(queueName);
            if (client == null) return null;
            var messages = await client.PeekMessagesAsync(maxMessages);
            return messages.Value;
        }

        public async Task<QueueMessage> ReceiveMessageAsync(string queueName = "")
        {
            var client = await GetClientAsync(queueName);
            if (client == null) return null;
            var message = await client.ReceiveMessageAsync();
            if (message != null && message.Value != null)
                return message.Value;

            return null;
        }

        public async Task<IEnumerable<QueueMessage>> ReceiveMessagesAsync(string queueName = "", int? maxMessages = default)
        {
            var client = await GetClientAsync(queueName);
            if (client == null) return null;
            var message = await client.ReceiveMessagesAsync(maxMessages);
            return message.Value;
        }

        public async Task<SendReceipt> SendMessageAsync(string queueName = "", string content = "")
        {
            var client = await GetClientAsync(queueName);
            if (client == null) return null;
            var message = await client.SendMessageAsync(content);
            if (message != null && message.Value != null)
                return message.Value;

            return null;
        }
    }
}
#endif
