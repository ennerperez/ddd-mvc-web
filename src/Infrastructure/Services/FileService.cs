#if USING_BLOBS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

// ReSharper disable InconsistentNaming

namespace Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        
        private List<BlobContainerClient> _clients;

        public FileService(IConfiguration configuration, ILoggerFactory logger)
        {
            _configuration = configuration;
            _logger = logger.CreateLogger(GetType());
            _clients = new List<BlobContainerClient>();
        }

        public string ContainerName { get; set; }
        public bool CreateIfNotExists { get; set; }

        public async Task<BlobContainerClient> GetClientAsync(string containerName = "", CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(containerName)) containerName = ContainerName;
            var client = _clients.FirstOrDefault(c => c.Name == containerName); 
            if (client != null)
                return client;

            try
            {
                _logger.LogInformation("Initializing [{ContainerName}] storage client", containerName);
                var connectionString = _configuration["AzureSettings:Storage:ConnectionString"];
                client = new BlobContainerClient(connectionString, containerName);
                await client.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
                _clients.Add(client);

                return client;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Message}", e.Message);
                throw new TypeInitializationException(GetType().FullName, new Exception($"Could not initialize the [{containerName}] storage client.", e));
            }
            
        }

        private string NormalizePath(string targetPath)
        {
            if (targetPath.StartsWith(@"app/") || targetPath.StartsWith(@"app\")) targetPath = targetPath.Substring(4);
            targetPath = targetPath.Replace(Path.DirectorySeparatorChar, '/');

            return targetPath;
        }

        private string NormalizeFilePath(string targetPath)
        {
            var fileName = Path.GetFileName(targetPath);
            targetPath = Path.GetDirectoryName(targetPath);
            if (targetPath != null && (targetPath.StartsWith(@"app/") || targetPath.StartsWith(@"app\"))) targetPath = targetPath.Substring(4);
            if (targetPath != null)
            {
                targetPath = targetPath.Replace(Path.DirectorySeparatorChar, '/');
                targetPath = Path.Combine(targetPath, fileName);

                return targetPath;
            }

            throw new NullReferenceException();
        }

        private async Task InternalWriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default, bool overwrite = false)
        {
            var _client = await GetClientAsync(string.Empty, cancellationToken);

            path = NormalizeFilePath(path);

            if (!overwrite)
            {
                var blob = _client.GetBlobClient(path);
                if (blob != null)
                    if (await blob.ExistsAsync())
                        return;
            }

            var data = new MemoryStream(bytes);
            if (overwrite) await _client.DeleteBlobIfExistsAsync(path, cancellationToken: cancellationToken);
            data.Position = 0;
            await _client.UploadBlobAsync(path, data, cancellationToken);
        }

        private async Task InternalWriteAllTextAsync(string path, string contents, Encoding encoding, CancellationToken cancellationToken = default, bool overwrite = false)
            => await InternalWriteAllBytesAsync(path, encoding.GetBytes(contents), cancellationToken, overwrite);

        private async Task InternalWriteAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default, bool overwrite = true)
            => await InternalWriteAllTextAsync(path, string.Join(Environment.NewLine, contents), encoding, cancellationToken);
        
        private async Task<byte[]> InternalReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
        {
            var _client = await GetClientAsync(string.Empty, cancellationToken);

            path = NormalizeFilePath(path);

            var blob = _client.GetBlobClient(path);
            if (blob != null)
                if (await blob.ExistsAsync(cancellationToken))
                {
                    byte[] result;
                    var download = blob.Download();
                    var downloadPath = Path.GetTempFileName();
                    using (var file = File.OpenWrite(downloadPath))
                        await download.Value.Content.CopyToAsync(file, cancellationToken);

                    result = await File.ReadAllBytesAsync(downloadPath, cancellationToken);
                    try
                    {
                        File.Delete(downloadPath);
                    }
                    catch
                    {
                        // ignored
                    }

                    return result;
                }

            return null;
        }

        private async Task<string> InternalReadAllTextAsync(string path, Encoding encoding, CancellationToken cancellationToken = default)
        {
            var _client = await GetClientAsync(string.Empty, cancellationToken);

            path = NormalizeFilePath(path);

            var blob = _client.GetBlobClient(path);
            if (blob != null)
                if (await blob.ExistsAsync(cancellationToken))
                {
                    string result;
                    var download = blob.Download();
                    var downloadPath = Path.GetTempFileName();
                    using (var file = File.OpenWrite(downloadPath))
                        await download.Value.Content.CopyToAsync(file, cancellationToken);

                    result = await File.ReadAllTextAsync(downloadPath, encoding, cancellationToken);
                    try
                    {
                        File.Delete(downloadPath);
                    }
                    catch
                    {
                        // ignored
                    }

                    return result;
                }

            return string.Empty;
        }

        private async Task<string[]> InternalReadAllLinesAsync(string path, Encoding encoding, CancellationToken cancellationToken = default)
        {
            var _client = await GetClientAsync(string.Empty, cancellationToken);

            path = NormalizeFilePath(path);

            var blob = _client.GetBlobClient(path);
            if (blob != null)
                if (await blob.ExistsAsync(cancellationToken))
                {
                    string[] result;
                    var download = blob.Download();
                    var downloadPath = Path.GetTempFileName();
                    using (var file = File.OpenWrite(downloadPath))
                        await download.Value.Content.CopyToAsync(file, cancellationToken);

                    result = await File.ReadAllLinesAsync(downloadPath, encoding, cancellationToken);
                    try
                    {
                        File.Delete(downloadPath);
                    }
                    catch
                    {
                        // ignored
                    }

                    return result;
                }

            return null;
        }

        
        private async Task<bool> InternalExistsAsync(string path, CancellationToken cancellationToken = default)
        {
            var _client = await GetClientAsync(string.Empty, cancellationToken);
            
            var blob = _client.GetBlobClient(path);
            if (blob != null) return await blob.ExistsAsync(cancellationToken);
            
            return false;
        }
        
        //private const int MaxByteArrayLength = 0x7FFFFFC7;
        private static Encoding s_UTF8NoBOM;
        private static Encoding UTF8NoBOM => s_UTF8NoBOM ?? (s_UTF8NoBOM = new UTF8Encoding(false, true));

        internal const int DefaultBufferSize = 4096;

        public StreamReader OpenText(string path)
        {
            throw new NotImplementedException();
        }

        public StreamWriter CreateText(string path)
        {
            throw new NotImplementedException();
        }

        public StreamWriter AppendText(string path)
        {
            throw new NotImplementedException();
        }

        public void Copy(string sourceFileName, string destFileName)
            => Copy(sourceFileName, destFileName, true);

        public void Copy(string sourceFileName, string destFileName, bool overwrite)
        {
            throw new NotImplementedException();
        }

        public FileStream Create(string path)
            => Create(path, DefaultBufferSize);

        public FileStream Create(string path, int bufferSize)
            => Create(path, bufferSize, FileOptions.None);

        public FileStream Create(string path, int bufferSize, FileOptions options)
        {
            throw new NotImplementedException();
        }

        public void Delete(string path)
        {
            throw new NotImplementedException();
        }

        public bool Exists(string path)
        {
            throw new NotImplementedException();
        }

        public FileStream Open(string path, FileMode mode)
        {
            throw new NotImplementedException();
        }

        public FileStream Open(string path, FileMode mode, FileAccess access)
        {
            throw new NotImplementedException();
        }

        public FileStream Open(string path, FileMode mode, FileAccess access, FileShare share)
        {
            throw new NotImplementedException();
        }

        public void SetCreationTime(string path, DateTime creationTime)
        {
            throw new NotImplementedException();
        }

        public void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
        {
            throw new NotImplementedException();
        }

        public DateTime GetCreationTime(string path)
        {
            throw new NotImplementedException();
        }

        public DateTime GetCreationTimeUtc(string path)
        {
            throw new NotImplementedException();
        }

        public void SetLastAccessTime(string path, DateTime lastAccessTime)
        {
            throw new NotImplementedException();
        }

        public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
        {
            throw new NotImplementedException();
        }

        public DateTime GetLastAccessTime(string path)
        {
            throw new NotImplementedException();
        }

        public DateTime GetLastAccessTimeUtc(string path)
        {
            throw new NotImplementedException();
        }

        public void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            throw new NotImplementedException();
        }

        public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            throw new NotImplementedException();
        }

        public DateTime GetLastWriteTime(string path)
        {
            throw new NotImplementedException();
        }

        public DateTime GetLastWriteTimeUtc(string path)
        {
            throw new NotImplementedException();
        }

        public FileAttributes GetAttributes(string path)
        {
            throw new NotImplementedException();
        }

        public void SetAttributes(string path, FileAttributes fileAttributes)
        {
            throw new NotImplementedException();
        }

        public FileStream OpenRead(string path)
        {
            throw new NotImplementedException();
        }

        public FileStream OpenWrite(string path)
        {
            throw new NotImplementedException();
        }

        public string ReadAllText(string path)
        {
            throw new NotImplementedException();
        }

        public string ReadAllText(string path, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        public void WriteAllText(string path, string contents)
        {
            throw new NotImplementedException();
        }

        public void WriteAllText(string path, string contents, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        public byte[] ReadAllBytes(string path)
        {
            throw new NotImplementedException();
        }

        public void WriteAllBytes(string path, byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public string[] ReadAllLines(string path)
        {
            throw new NotImplementedException();
        }

        public string[] ReadAllLines(string path, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> ReadLines(string path)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> ReadLines(string path, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        public void WriteAllLines(string path, string[] contents)
        {
            throw new NotImplementedException();
        }

        public void WriteAllLines(string path, IEnumerable<string> contents)
        {
            throw new NotImplementedException();
        }

        public void WriteAllLines(string path, string[] contents, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        public void WriteAllLines(string path, IEnumerable<string> contents, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        public void AppendAllText(string path, string contents)
        {
            throw new NotImplementedException();
        }

        public void AppendAllText(string path, string contents, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        public void AppendAllLines(string path, IEnumerable<string> contents)
        {
            throw new NotImplementedException();
        }

        public void AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        public void Replace(string sourceFileName, string destinationFileName, string destinationBackupFileName)
        {
            throw new NotImplementedException();
        }

        public void Replace(string sourceFileName, string destinationFileName, string destinationBackupFileName, bool ignoreMetadataErrors)
        {
            throw new NotImplementedException();
        }

        public void Move(string sourceFileName, string destFileName)
        {
            throw new NotImplementedException();
        }

        public void Move(string sourceFileName, string destFileName, bool overwrite)
        {
            throw new NotImplementedException();
        }

        public void Encrypt(string path)
        {
            throw new NotImplementedException();
        }

        public void Decrypt(string path)
        {
            throw new NotImplementedException();
        }

        public Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
            => ReadAllTextAsync(path, Encoding.UTF8, cancellationToken);

        public Task<string> ReadAllTextAsync(string path, Encoding encoding, CancellationToken cancellationToken = default)
            => InternalReadAllTextAsync(path, encoding, cancellationToken);

        public Task WriteAllTextAsync(string path, string contents, CancellationToken cancellationToken = default)
            => WriteAllTextAsync(path, contents, UTF8NoBOM, cancellationToken);

        public Task WriteAllTextAsync(string path, string contents, Encoding encoding, CancellationToken cancellationToken = default)
            => InternalWriteAllTextAsync(path, contents, encoding, cancellationToken, true);

        public Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
            => InternalReadAllBytesAsync(path, cancellationToken);

        public Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
            => InternalWriteAllBytesAsync(path, bytes, cancellationToken, true);

        public Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
            => InternalExistsAsync(path, cancellationToken);

        public Task<string[]> ReadAllLinesAsync(string path, CancellationToken cancellationToken = default)
            => ReadAllLinesAsync(path, Encoding.UTF8, cancellationToken);

        public Task<string[]> ReadAllLinesAsync(string path, Encoding encoding, CancellationToken cancellationToken = default)
            => InternalReadAllLinesAsync(path, encoding, cancellationToken);

        public Task WriteAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken = default)
            => WriteAllLinesAsync(path, contents, UTF8NoBOM, cancellationToken);

        public Task WriteAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default)
            => InternalWriteAllLinesAsync(path, contents, encoding, cancellationToken);

        public Task AppendAllTextAsync(string path, string contents, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task AppendAllTextAsync(string path, string contents, Encoding encoding, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task AppendAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task AppendAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
#endif
