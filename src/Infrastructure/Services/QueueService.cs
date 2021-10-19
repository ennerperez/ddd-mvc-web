using System;
using System.Collections.Generic;
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

        public QueueService(IConfiguration configuration, ILoggerFactory logger)
        {
            _configuration = configuration;
            _logger = logger.CreateLogger(GetType());
        }

        private string _queueName;

        public string QueueName
        {
            get => _queueName;
            set
            {
                if (value != _queueName)
                {
                    _queueName = value;
                    Deinitialize();
                }
            }
        }

        private string _connectionString;

        private QueueClient _client;

        public bool Initialized { get; private set; }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (Initialized) return;

            _logger.LogInformation($"Initializing Queue Client");

            try
            {
                _connectionString = _configuration["AzureSettings:Storage:ConnectionString"];
                _client = new QueueClient(_connectionString, QueueName);
                await _client.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

                Initialized = true;
            }
            catch (Exception e)
            {
                Initialized = false;
                _logger.LogError(e, e.Message);
            }

            if (!Initialized)
                throw new TypeInitializationException(this.GetType().FullName, new Exception("Could not connect to the queue client."));
        }

        public void Deinitialize()
        {
            _logger.LogInformation($"Deinitializing Queue Client");
            Initialized = false;
            _client = null;
        }

        public async Task<IEnumerable<PeekedMessage>> GetMessagesAsync()
        {
            var messages = await _client.PeekMessagesAsync();
            return messages.Value;
        }

        public async Task<QueueMessage> ReceiveMessageAsync()
        {
            var message = await _client.ReceiveMessageAsync();
            if (message != null && message.Value != null)
            {
                return message.Value;
            }
            return null;
        }
        
        public async Task<SendReceipt> SendMessageAsync(string content)
        {
            var message = await _client.SendMessageAsync(content);
            if (message != null && message.Value != null)
            {
                return message.Value;
            }
            return null;
        }
    }
}
