#if USING_BLOBS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

// ReSharper disable UnusedMember.Local
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
// ReSharper disable NotNullOrRequiredMemberIsNotInitialized

namespace Infrastructure.Services
{
    public class DirectoryService : IDirectoryService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        private readonly List<BlobContainerClient> _clients;

        public DirectoryService(IConfiguration configuration, ILoggerFactory logger)
        {
            _configuration = configuration;
            _logger = logger.CreateLogger(GetType());
            _clients = [];
        }

        public string ContainerName { get; set; }
        public bool CreateIfNotExists { get; set; }

        public string DirectoryExtension { get; set; } = ".dir";

        private BlobContainerClient GetClient(string containerName = "")
        {
            if (string.IsNullOrWhiteSpace(containerName))
            {
                containerName = ContainerName;
            }

            var client = _clients.FirstOrDefault(c => c.Name == containerName);
            if (client != null)
            {
                return client;
            }

            try
            {
                _logger.LogInformation(message: "Initializing [{ContainerName}] storage client", containerName);
                var connectionString = _configuration["AzureSettings:Storage:ConnectionString"];
                client = new BlobContainerClient(connectionString, containerName);
                if (CreateIfNotExists)
                {
                    client.CreateIfNotExists();
                }

                _clients.Add(client);

                return client;
            }
            catch (Exception e)
            {
                _logger.LogError(e, message: "{Message}", e.Message);
                throw new TypeInitializationException(GetType().FullName, new Exception($"Could not initialize the [{containerName}] storage client.", e));
            }
        }
        private static string NormalizePath(string targetPath)
        {
            if (targetPath.StartsWith("app/") || targetPath.StartsWith(@"app\"))
            {
                targetPath = targetPath[4..];
            }

            targetPath = targetPath.Replace(Path.DirectorySeparatorChar, '/');
            return targetPath;
        }

        private static string NormalizeFilePath(string targetPath)
        {
            var fileName = Path.GetFileName(targetPath);
            targetPath = Path.GetDirectoryName(targetPath);
            if (targetPath != null && (targetPath.StartsWith("app/") || targetPath.StartsWith(@"app\")))
            {
                targetPath = targetPath[4..];
            }

            if (targetPath == null)
            {
                return null;
            }

            targetPath = targetPath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            targetPath = Path.Combine(targetPath, fileName);

            return targetPath;
        }


        public DirectoryInfo GetParent(string path)
        {
            return new DirectoryInfo(Path.GetRelativePath(Directory.GetCurrentDirectory(), new DirectoryInfo(path).Parent?.FullName ?? throw new DirectoryNotFoundException()));
        }

        public DirectoryInfo CreateDirectory(string path)
        {
            var client = GetClient();
            path = NormalizeFilePath(path);

            var folderPath = Path.Combine(path, DirectoryExtension);
            var blob = client.GetBlobClient(folderPath);
            if (blob == null || !blob.Exists())
            {
                client.UploadBlob(folderPath, new BinaryData([
                    1
                ]));
            }

            return new DirectoryInfo(path);
        }

        public bool Exists(string path)
        {
            var client = GetClient();
            path = NormalizeFilePath(path);

            var folderPath = Path.Combine(path, DirectoryExtension);
            var blob = client.GetBlobClient(folderPath);
            return blob.Exists();
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

        public string[] GetFiles(string path)
        {
            return InternalGetFiles(path, string.Empty, SearchOption.TopDirectoryOnly, null);
        }

        public string[] GetFiles(string path, string searchPattern)
        {
            return InternalGetFiles(path, searchPattern, SearchOption.TopDirectoryOnly, null);
        }

        public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            return InternalGetFiles(path, searchPattern, searchOption, null);
        }

        public string[] GetFiles(string path, string searchPattern, EnumerationOptions enumerationOptions)
        {
            return InternalGetFiles(path, searchPattern, SearchOption.TopDirectoryOnly, enumerationOptions);
        }

        private string[] InternalGetFiles(string path, string searchPattern, SearchOption searchOption, EnumerationOptions enumerationOptions)
        {
            if (enumerationOptions != null)
            {
                throw new NotImplementedException("Unable to use with EnumerationOptions");
            }

            var client = GetClient();
            path = NormalizeFilePath(path);

            var items = client.GetBlobsByHierarchy(BlobTraits.None, BlobStates.None, null, path).ToArray();
            if (!string.IsNullOrWhiteSpace(searchPattern))
            {
                items = items.Where(m => m.Blob.Name.Contains(searchPattern)).ToArray();
            }

            var files = items.Where(m => m.IsBlob && m.Blob.Name != $"{path}{Path.AltDirectorySeparatorChar}{DirectoryExtension}");

            List<string> results = files.Select(m => m.Blob.Name).ToList();

            if (searchOption == SearchOption.TopDirectoryOnly)
            {
                results = results.Where(m => Path.GetDirectoryName(m) == path).ToList();
            }

            return results.ToArray();
        }


        public string[] GetDirectories(string path)
        {
            return InternalGetDirectories(path, string.Empty, SearchOption.TopDirectoryOnly, null);
        }

        public string[] GetDirectories(string path, string searchPattern)
        {
            return InternalGetDirectories(path, searchPattern, SearchOption.TopDirectoryOnly, null);
        }

        public string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
        {
            return InternalGetDirectories(path, searchPattern, searchOption, null);
        }

        public string[] GetDirectories(string path, string searchPattern, EnumerationOptions enumerationOptions)
        {
            return InternalGetDirectories(path, searchPattern, SearchOption.TopDirectoryOnly, enumerationOptions);
        }

        private string[] InternalGetDirectories(string path, string searchPattern, SearchOption searchOption, EnumerationOptions enumerationOptions)
        {
            return InternalEnumerateDirectories(path, searchPattern, searchOption, enumerationOptions).ToArray();
        }

        public string[] GetFileSystemEntries(string path)
        {
            throw new NotImplementedException();
        }

        public string[] GetFileSystemEntries(string path, string searchPattern)
        {
            throw new NotImplementedException();
        }

        public string[] GetFileSystemEntries(string path, string searchPattern, SearchOption searchOption)
        {
            throw new NotImplementedException();
        }

        public string[] GetFileSystemEntries(string path, string searchPattern, EnumerationOptions enumerationOptions)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> EnumerateDirectories(string path)
        {
            return InternalEnumerateDirectories(path, string.Empty, SearchOption.TopDirectoryOnly, null);
        }

        public IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
        {
            return InternalEnumerateDirectories(path, searchPattern, SearchOption.TopDirectoryOnly, null);
        }

        public IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
        {
            return InternalEnumerateDirectories(path, searchPattern, searchOption, null);
        }

        public IEnumerable<string> EnumerateDirectories(string path, string searchPattern, EnumerationOptions enumerationOptions)
        {
            return InternalEnumerateDirectories(path, searchPattern, SearchOption.TopDirectoryOnly, enumerationOptions);
        }

        public IEnumerable<string> EnumerateFiles(string path)
        {
            return InternalEnumerateDirectories(path, string.Empty, SearchOption.TopDirectoryOnly, null);
        }

        public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
        {
            return InternalEnumerateDirectories(path, searchPattern, SearchOption.TopDirectoryOnly, null);
        }

        public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
        {
            return InternalEnumerateDirectories(path, searchPattern, searchOption, null);
        }

        public IEnumerable<string> EnumerateFiles(string path, string searchPattern, EnumerationOptions enumerationOptions)
        {
            return InternalEnumerateDirectories(path, searchPattern, SearchOption.TopDirectoryOnly, null);
        }

        private IEnumerable<string> InternalEnumerateDirectories(string path, string searchPattern, SearchOption searchOption, EnumerationOptions enumerationOptions)
        {
            if (enumerationOptions != null)
            {
                throw new NotImplementedException("Unable to use with EnumerationOptions");
            }

            var client = GetClient();
            path = NormalizeFilePath(path);

            var items = client.GetBlobsByHierarchy(BlobTraits.None, BlobStates.None, null, path).ToArray();
            if (!string.IsNullOrWhiteSpace(searchPattern))
            {
                items = items.Where(m => m.Blob.Name.Contains(searchPattern)).ToArray();
            }

            var folders = items.Where(m => m.IsBlob && m.Blob.Name.EndsWith($"{Path.AltDirectorySeparatorChar}{DirectoryExtension}"));

            List<string> results;
            var dirResults = folders.Select(m =>
                Path.GetRelativePath(Directory.GetCurrentDirectory(), new FileInfo(m.Blob.Name).Directory?.FullName ?? throw new DirectoryNotFoundException())).ToList();

            if (searchOption == SearchOption.TopDirectoryOnly)
            {
                results = dirResults.Where(m =>
                {
                    var d = new DirectoryInfo(m);
                    return d.Parent == null || d.Parent.FullName == Directory.GetCurrentDirectory();
                }).ToList();
            }
            else
            {
                results = dirResults;
            }

            return results.AsEnumerable();
        }

        public IEnumerable<string> EnumerateFileSystemEntries(string path)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, SearchOption searchOption)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, EnumerationOptions enumerationOptions)
        {
            throw new NotImplementedException();
        }

        public string GetDirectoryRoot(string path)
        {
            return ".";
        }

        public string GetCurrentDirectory()
        {
            return ContainerName;
        }

        public void SetCurrentDirectory(string path)
        {
            ContainerName = path;
        }

        public void Move(string sourceDirName, string destDirName)
        {
            throw new NotImplementedException();
        }

        public void Delete(string path)
        {
            InternalDelete(path, false);
        }

        public void Delete(string path, bool recursive)
        {
            InternalDelete(path, recursive);
        }

        private void InternalDelete(string path, bool recursive)
        {
            var client = GetClient();
            path = NormalizeFilePath(path);

            var items = client.GetBlobsByHierarchy(BlobTraits.None, BlobStates.None, null, path).ToArray();
            if (!recursive && items.Any(m => m.IsBlob && !m.Blob.Name.EndsWith($"{Path.AltDirectorySeparatorChar}{DirectoryExtension}")))
            {
                throw new Exception("The directory its not empty");
            }

            foreach (var item in items)
            {
                client.DeleteBlobIfExists(item.Blob.Name);
            }
        }


        public string[] GetLogicalDrives()
        {
            throw new NotImplementedException();
        }

        public FileSystemInfo CreateSymbolicLink(string path, string pathToTarget)
        {
            throw new NotImplementedException();
        }

        public FileSystemInfo ResolveLinkTarget(string linkPath, bool returnFinalTarget)
        {
            throw new NotImplementedException();
        }
    }
}

#endif
