using System;
using System.Threading;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class VaultService : IVaultService<KeyVaultSecret>
    {

        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        private SecretClient _client;

        public VaultService(IConfiguration configuration, ILoggerFactory logger)
        {
            _configuration = configuration;
            _logger = logger.CreateLogger(GetType());
        }

        private SecretClient GetClient()
        {
            if (_client != null)
                return _client;

            try
            {
                _logger.LogInformation("Initializing vault client");
                var vaultUrl = _configuration["AzureSettings:Vault:Uri"];
                _client = new SecretClient(vaultUri: new Uri(vaultUrl), credential: new DefaultAzureCredential());
                return _client;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Message}", e.Message);
                throw new TypeInitializationException(GetType().FullName, new Exception($"Could not initialize the vault client.", e));
            }
        }
        public KeyVaultSecret GetSecret(string key, string version = null, CancellationToken cancellationToken = default)
        {
            var client = GetClient();
            return client.GetSecret(key, version, cancellationToken);
        }
        public KeyVaultSecret SetSecret(string key, string value, CancellationToken cancellationToken = default)
        {
            var client = GetClient();
            return client.SetSecret(key, value, cancellationToken);
        }
    }
}
