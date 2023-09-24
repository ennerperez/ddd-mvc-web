#if USING_BLOBS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Infrastructure.Interfaces;
using Infrastructure.Records;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedParameter.Local

namespace Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        private List<BlobContainerClient> clients;

        public FileService(IConfiguration configuration, ILoggerFactory logger)
        {
            _configuration = configuration;
            _logger = logger.CreateLogger(GetType());
            clients = new List<BlobContainerClient>();
        }

        public string ContainerName { get; set; }
        public bool CreateIfNotExists { get; set; }

        private BlobContainerClient GetClient(string containerName = "")
        {
            if (string.IsNullOrWhiteSpace(containerName)) containerName = ContainerName;
            var client = clients.FirstOrDefault(c => c.Name == containerName);
            if (client != null)
                return client;

            try
            {
                _logger.LogInformation("Initializing [{ContainerName}] storage client", containerName);
                var connectionString = _configuration["AzureSettings:Storage:ConnectionString"];
                client = new BlobContainerClient(connectionString, containerName);
                if (CreateIfNotExists) client.CreateIfNotExists();
                clients.Add(client);

                return client;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Message}", e.Message);
                throw new TypeInitializationException(GetType().FullName, new Exception($"Could not initialize the [{containerName}] storage client.", e));
            }

        }
        private async Task<BlobContainerClient> GetClientAsync(string containerName = "", CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(containerName)) containerName = ContainerName;
            var client = clients.FirstOrDefault(c => c.Name == containerName);
            if (client != null)
                return client;

            try
            {
                _logger.LogInformation("Initializing [{ContainerName}] storage client", containerName);
                var connectionString = _configuration["AzureSettings:Storage:ConnectionString"];
                client = new BlobContainerClient(connectionString, containerName);
                if (CreateIfNotExists) await client.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
                clients.Add(client);

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
            }
            return targetPath;
        }

        #region Internals

        private async Task InternalWriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default, bool overwrite = false)
        {
            var client = await GetClientAsync(cancellationToken: cancellationToken);

            path = NormalizeFilePath(path);

            if (!overwrite)
            {
                var blob = client.GetBlobClient(path);
                if (blob != null)
                    if (await blob.ExistsAsync())
                        return;
            }

            var data = new MemoryStream(bytes);
            if (overwrite) await client.DeleteBlobIfExistsAsync(path, cancellationToken: cancellationToken);
            data.Position = 0;
            await client.UploadBlobAsync(path, data, cancellationToken);
        }
        private async Task<string> InternalReadAllTextAsync(string path, Encoding encoding, CancellationToken cancellationToken = default)
        {
            var client = await GetClientAsync(cancellationToken: cancellationToken);

            path = NormalizeFilePath(path);

            var blob = client.GetBlobClient(path);
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
        private async Task InternalWriteAllTextAsync(string path, string contents, Encoding encoding, CancellationToken cancellationToken = default, bool overwrite = false)
            => await InternalWriteAllBytesAsync(path, encoding.GetBytes(contents), cancellationToken, overwrite);
        private void InternalWriteAllText(string path, string contents, Encoding encoding, bool overwrite = false)
            => InternalWriteAllBytes(path, encoding.GetBytes(contents), overwrite);
        private async Task InternalWriteAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default, bool overwrite = true)
            => await InternalWriteAllTextAsync(path, string.Join(Environment.NewLine, contents), encoding, cancellationToken);
        private void InternalWriteAllLines(string path, IEnumerable<string> contents, Encoding encoding, bool overwrite = true)
            => InternalWriteAllText(path, string.Join(Environment.NewLine, contents), encoding);
        private async Task<byte[]> InternalReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
        {
            var client = await GetClientAsync(cancellationToken: cancellationToken);

            path = NormalizeFilePath(path);

            var blob = client.GetBlobClient(path);
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
        private async Task<string[]> InternalReadAllLinesAsync(string path, Encoding encoding, CancellationToken cancellationToken = default)
        {
            var client = await GetClientAsync(cancellationToken: cancellationToken);

            path = NormalizeFilePath(path);

            var blob = client.GetBlobClient(path);
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
        private string[] InternalReadAllLines(string path, Encoding encoding)
        {
            var client = GetClient();

            path = NormalizeFilePath(path);

            var blob = client.GetBlobClient(path);
            if (blob != null)
                if (blob.Exists())
                {
                    string[] result;
                    var download = blob.Download();
                    var downloadPath = Path.GetTempFileName();
                    using (var file = File.OpenWrite(downloadPath))
                        download.Value.Content.CopyTo(file);

                    result = File.ReadAllLines(downloadPath, encoding);
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
            var client = await GetClientAsync(cancellationToken: cancellationToken);
            path = NormalizeFilePath(path);
            var blob = client.GetBlobClient(path);
            if (blob != null) return await blob.ExistsAsync(cancellationToken);

            return false;
        }
        private void InternalCopy(string sourceFileName, string destFileName, bool overwrite)
        {
            var client = GetClient();

            sourceFileName = NormalizeFilePath(sourceFileName);
            destFileName = NormalizeFilePath(destFileName);

            var sourceBlob = client.GetBlobClient(sourceFileName);
            var destBlob = client.GetBlobClient(destFileName);

            if (sourceBlob != null)
                using (var stream = sourceBlob.OpenRead())
                {
                    using (var writer = destBlob.OpenWrite(overwrite))
                    {
                        using (var reader = new StreamReader(stream))
                            while (!reader.EndOfStream)
                                stream.CopyTo(writer);
                    }
                }
        }
        private FileStream InternalCreate(string path, int bufferSize, FileOptions options, bool overwrite = false)
         => throw new NotImplementedException();
        private FileStream InternalOpen(string path, FileMode mode, FileAccess access, FileShare share)
        {
            var client = GetClient();

            path = NormalizeFilePath(path);

            var blob = client.GetBlobClient(path);

            if (blob != null)
                using (var stream = blob.OpenRead())
                {
                    var tempFile = Path.GetTempFileName();
                    using (var writer = File.Open(tempFile, FileMode.OpenOrCreate))
                    {
                        using (var reader = new StreamReader(stream))
                            while (!reader.EndOfStream)
                                stream.CopyTo(writer);
                        return writer;
                    }
                }

            return null;
        }
        private void InternalDelete(string path)
        {
            var client = GetClient();
            path = NormalizeFilePath(path);
            client.DeleteBlobIfExists(path);
        }
        private bool InternalExists(string path)
        {
            var client = GetClient();
            path = NormalizeFilePath(path);
            var blob = client.GetBlobClient(path);
            return blob.Exists();
        }
        private string InternalReadAllText(string path, Encoding encoding)
        {
            var client = GetClient();

            path = NormalizeFilePath(path);

            var blob = client.GetBlobClient(path);
            if (blob != null)
                if (blob.Exists())
                {
                    string result;
                    var download = blob.Download();
                    var downloadPath = Path.GetTempFileName();
                    using (var file = File.OpenWrite(downloadPath))
                        download.Value.Content.CopyToAsync(file);

                    result = File.ReadAllText(downloadPath, encoding);
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
        private void InternalWriteAllBytes(string path, byte[] bytes, bool overwrite = false)
        {
            var client = GetClient();

            path = NormalizeFilePath(path);

            if (!overwrite)
            {
                var blob = client.GetBlobClient(path);
                if (blob != null)
                    if (blob.Exists())
                        return;
            }

            var data = new MemoryStream(bytes);
            if (overwrite) client.DeleteBlobIfExistsAsync(path);
            data.Position = 0;
            client.UploadBlob(path, data);
        }
        private byte[] InternalReadAllBytes(string path)
        {
            var client = GetClient();

            path = NormalizeFilePath(path);

            var blob = client.GetBlobClient(path);
            if (blob != null)
                if (blob.Exists())
                {
                    byte[] result;
                    var download = blob.Download();
                    var downloadPath = Path.GetTempFileName();
                    using (var file = File.OpenWrite(downloadPath))
                        download.Value.Content.CopyTo(file);

                    result = File.ReadAllBytes(downloadPath);
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

        #endregion

        // private const int MaxByteArrayLength = 0x7FFFFFC7;
        private static Encoding s_UTF8NoBOM;
        private static Encoding UTF8NoBOM => s_UTF8NoBOM ?? (s_UTF8NoBOM = new UTF8Encoding(false, true));

        internal const int DefaultBufferSize = 4096;

        public StreamReader OpenText(string path)
            => throw new NotImplementedException();
        public StreamWriter CreateText(string path)
            => throw new NotImplementedException();
        public StreamWriter AppendText(string path)
            => throw new NotImplementedException();

        public void Copy(string sourceFileName, string destFileName)
            => InternalCopy(sourceFileName, destFileName, true);
        public void Copy(string sourceFileName, string destFileName, bool overwrite)
            => InternalCopy(sourceFileName, destFileName, true);
        public FileStream Create(string path)
            => InternalCreate(path, DefaultBufferSize, FileOptions.None);
        public FileStream Create(string path, int bufferSize)
            => InternalCreate(path, bufferSize, FileOptions.None);
        public FileStream Create(string path, int bufferSize, FileOptions options)
            => InternalCreate(path, bufferSize, FileOptions.None);
        public void Delete(string path)
            => InternalDelete(path);
        public bool Exists(string path)
            => InternalExists(path);
        public FileStream OpenRead(string path)
            => InternalOpen(path, FileMode.Open, FileAccess.Read, FileShare.None);
        public FileStream Open(string path, FileMode mode)
            => InternalOpen(path, mode, FileAccess.Read, FileShare.None);
        public FileStream Open(string path, FileMode mode, FileAccess access)
            => InternalOpen(path, mode, access, FileShare.None);
        public FileStream Open(string path, FileMode mode, FileAccess access, FileShare share)
            => InternalOpen(path, mode, access, share);

        public void SetCreationTime(string path, DateTime creationTime)
            => throw new NotImplementedException();
        public void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
            => throw new NotImplementedException();
        public DateTime GetCreationTime(string path)
            => throw new NotImplementedException();
        public DateTime GetCreationTimeUtc(string path)
            => throw new NotImplementedException();
        public void SetLastAccessTime(string path, DateTime lastAccessTime)
            => throw new NotImplementedException();
        public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
            => throw new NotImplementedException();
        public DateTime GetLastAccessTime(string path)
            => throw new NotImplementedException();
        public DateTime GetLastAccessTimeUtc(string path)
            => throw new NotImplementedException();
        public void SetLastWriteTime(string path, DateTime lastWriteTime)
            => throw new NotImplementedException();
        public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
            => throw new NotImplementedException();
        public DateTime GetLastWriteTime(string path)
            => throw new NotImplementedException();
        public DateTime GetLastWriteTimeUtc(string path)
            => throw new NotImplementedException();
        public FileAttributes GetAttributes(string path)
            => throw new NotImplementedException();
        public void SetAttributes(string path, FileAttributes fileAttributes)
            => throw new NotImplementedException();
        public FileStream OpenWrite(string path)
            => throw new NotImplementedException();


        public string ReadAllText(string path)
            => InternalReadAllText(path, Encoding.UTF8);
        public string ReadAllText(string path, Encoding encoding)
            => InternalReadAllText(path, encoding);
        public void WriteAllText(string path, string contents)
            => InternalWriteAllBytes(path, Encoding.UTF8.GetBytes(contents));
        public void WriteAllText(string path, string contents, Encoding encoding)
            => InternalWriteAllBytes(path, encoding.GetBytes(contents));
        public byte[] ReadAllBytes(string path)
            => InternalReadAllBytes(path);
        public void WriteAllBytes(string path, byte[] bytes)
            => InternalWriteAllBytes(path, bytes);
        public string[] ReadAllLines(string path)
            => InternalReadAllLines(path, Encoding.Default);
        public string[] ReadAllLines(string path, Encoding encoding)
            => InternalReadAllLines(path, encoding);

        public IEnumerable<string> ReadLines(string path)
            => throw new NotImplementedException();
        public IEnumerable<string> ReadLines(string path, Encoding encoding)
            => throw new NotImplementedException();

        public void WriteAllLines(string path, string[] contents)
            => InternalWriteAllLines(path, contents, Encoding.Default);
        public void WriteAllLines(string path, IEnumerable<string> contents)
            => InternalWriteAllLines(path, contents, Encoding.Default);
        public void WriteAllLines(string path, string[] contents, Encoding encoding)
            => InternalWriteAllLines(path, contents, encoding);
        public void WriteAllLines(string path, IEnumerable<string> contents, Encoding encoding)
            => InternalWriteAllLines(path, contents, encoding);

        public void AppendAllText(string path, string contents)
            => throw new NotImplementedException();
        public void AppendAllText(string path, string contents, Encoding encoding)
            => throw new NotImplementedException();
        public void AppendAllLines(string path, IEnumerable<string> contents)
            => throw new NotImplementedException();
        public void AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding)
            => throw new NotImplementedException();
        public void Replace(string sourceFileName, string destinationFileName, string destinationBackupFileName)
            => throw new NotImplementedException();
        public void Replace(string sourceFileName, string destinationFileName, string destinationBackupFileName, bool ignoreMetadataErrors)
            => throw new NotImplementedException();
        public void Move(string sourceFileName, string destFileName)
            => throw new NotImplementedException();
        public void Move(string sourceFileName, string destFileName, bool overwrite)
            => throw new NotImplementedException();
        public void Encrypt(string path)
            => throw new NotImplementedException();

        public void Decrypt(string path)
            => throw new NotImplementedException();

        public Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
            => InternalReadAllTextAsync(path, Encoding.UTF8, cancellationToken);

        public Task<string> ReadAllTextAsync(string path, Encoding encoding, CancellationToken cancellationToken = default)
            => InternalReadAllTextAsync(path, encoding, cancellationToken);

        public Task WriteAllTextAsync(string path, string contents, CancellationToken cancellationToken = default)
            => InternalWriteAllTextAsync(path, contents, UTF8NoBOM, cancellationToken);

        public Task WriteAllTextAsync(string path, string contents, Encoding encoding, CancellationToken cancellationToken = default)
            => InternalWriteAllTextAsync(path, contents, encoding, cancellationToken, true);

        public Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
            => InternalReadAllBytesAsync(path, cancellationToken);

        public Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
            => InternalWriteAllBytesAsync(path, bytes, cancellationToken, true);

        public Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
            => InternalExistsAsync(path, cancellationToken);

        public Task<string[]> ReadAllLinesAsync(string path, CancellationToken cancellationToken = default)
            => InternalReadAllLinesAsync(path, Encoding.UTF8, cancellationToken);

        public Task<string[]> ReadAllLinesAsync(string path, Encoding encoding, CancellationToken cancellationToken = default)
            => InternalReadAllLinesAsync(path, encoding, cancellationToken);

        public Task WriteAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken = default)
            => InternalWriteAllLinesAsync(path, contents, UTF8NoBOM, cancellationToken);

        public Task WriteAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default)
            => InternalWriteAllLinesAsync(path, contents, encoding, cancellationToken);

        public Task AppendAllTextAsync(string path, string contents, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task AppendAllTextAsync(string path, string contents, Encoding encoding, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task AppendAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task AppendAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        #region Extended

        private async Task<Stream> InternalReadStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            var client = await GetClientAsync(cancellationToken: cancellationToken);

            path = NormalizeFilePath(path);

            var blob = client.GetBlobClient(path);

            if (blob != null)
                if (await blob.ExistsAsync(cancellationToken))
                {
                    var file = new MemoryStream();

                    await blob.DownloadToAsync(file, cancellationToken);

                    file.Seek(0, SeekOrigin.Begin);

                    return file;
                }

            return null;
        }

        public async Task DeleteAsync(string path, CancellationToken cancellationToken = default)
        {
            var client = await GetClientAsync(cancellationToken: cancellationToken);
            path = NormalizeFilePath(path);
            await client.DeleteBlobIfExistsAsync(path, cancellationToken: cancellationToken);
        }

        public Task<Stream> ReadStreamAsync(string path, CancellationToken cancellationToken = default)
            => InternalReadStreamAsync(path, cancellationToken);

        public Task<string[]> GetFilesAsync(string path, string searchPattern = ".*", SearchOption searchOption = SearchOption.TopDirectoryOnly, CancellationToken cancellationToken = default) =>
            InternalGetFilesAsync(path, searchPattern, searchOption, cancellationToken);

        public Task<FileRecord[]> GetFilesInfoAsync(string path, string searchPattern = ".*", SearchOption searchOption = SearchOption.TopDirectoryOnly, CancellationToken cancellationToken = default) =>
            InternalGetFilesInfoAsync(path, searchPattern, searchOption, cancellationToken);

        public Task<FileRecord> GetFileInfoAsync(string path, CancellationToken cancellationToken = default) =>
            InternalGetFileInfoAsync(path, cancellationToken);

        private async Task<string[]> InternalGetFilesAsync(string path, string searchPattern, SearchOption searchOption, CancellationToken cancellationToken)
        {
            var client = await GetClientAsync(cancellationToken: cancellationToken);

            path = NormalizePath(path + Path.AltDirectorySeparatorChar);
            var result = new List<string>();

            var items = client.GetBlobsByHierarchyAsync(BlobTraits.None, BlobStates.None, null, path, cancellationToken: cancellationToken);

            await foreach (var item in items)
                if (searchOption == SearchOption.TopDirectoryOnly)
                    result.Add(item.Blob.Name);

            if (result.Any())
                return result.ToArray();
            else
                return null;
        }

        private async Task<FileRecord[]> InternalGetFilesInfoAsync(string path, string searchPattern, SearchOption searchOption, CancellationToken cancellationToken)
        {
            var client = await GetClientAsync(cancellationToken: cancellationToken);

            path = NormalizePath(path);
            var result = new List<FileRecord>();

            var items = client.GetBlobsByHierarchyAsync(BlobTraits.Metadata, BlobStates.None, null, path, cancellationToken: cancellationToken);

            await foreach (var item in items)
                if (searchOption == SearchOption.TopDirectoryOnly)
                    result.Add(new FileRecord(item.Blob.Name, item.Blob.Properties.CreatedOn?.UtcDateTime ?? DateTime.MinValue, item.Blob.Properties.LastModified?.UtcDateTime, Path.GetExtension(item.Blob.Name), item.Blob.Properties.ContentLength));

            if (result.Any())
                return result.ToArray();
            else
                return null;
        }

        private async Task<FileRecord> InternalGetFileInfoAsync(string path, CancellationToken cancellationToken)
        {
            var client = await GetClientAsync(cancellationToken: cancellationToken);

            path = NormalizeFilePath(path);
            var result = new List<FileRecord>();

            var items = client.GetBlobsByHierarchyAsync(BlobTraits.Metadata, BlobStates.None, null, path, cancellationToken: cancellationToken);

            await foreach (var item in items)
                result.Add(new FileRecord(item.Blob.Name, item.Blob.Properties.CreatedOn?.UtcDateTime ?? DateTime.MinValue, item.Blob.Properties.LastModified?.UtcDateTime, Path.GetExtension(item.Blob.Name), item.Blob.Properties.ContentLength));

            if (result.Any())
                return result.FirstOrDefault();
            else
                return null;
        }

        #endregion

        #region URL

        public async Task<string> GetUrlAsync(string path, CancellationToken cancellationToken = default)
        {
            var client = await GetClientAsync(cancellationToken: cancellationToken);

            path = NormalizeFilePath(path);

            var blob = client.GetBlobClient(path);
            if (blob != null && blob.CanGenerateSasUri)
                return blob.Uri.ToString();

            return null;
        }
        public string GetUrl(string path)
        {
            var client = GetClient();

            path = NormalizeFilePath(path);

            var blob = client.GetBlobClient(path);
            if (blob != null && blob.CanGenerateSasUri)
                return blob.Uri.ToString();

            return null;
        }

        #endregion

    }
}
#endif
