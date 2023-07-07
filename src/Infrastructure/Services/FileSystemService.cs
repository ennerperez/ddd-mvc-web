using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Interfaces;
using Infrastructure.Records;

namespace Infrastructure.Services
{
    public class FileSystemService : IFileService, IDirectoryService
    {

        public string ContainerName { get; set; }
        public bool CreateIfNotExists { get; set; }
        public string DirectoryExtension { get; set; }

        #region FileService

        public StreamReader OpenText(string path)
        {
            path = ValidatePath(path);
            return File.OpenText(path);
        }
        public StreamWriter CreateText(string path)
        {
            path = ValidatePath(path);
            return File.CreateText(path);
        }
        public StreamWriter AppendText(string path)
        {
            path = ValidatePath(path);
            return File.AppendText(path);
        }
        public void Copy(string sourceFileName, string destFileName)
        {
            sourceFileName = ValidatePath(sourceFileName);
            destFileName = ValidatePath(destFileName);
            File.Copy(sourceFileName, destFileName);
        }
        public void Copy(string sourceFileName, string destFileName, bool overwrite)
        {
            sourceFileName = ValidatePath(sourceFileName);
            destFileName = ValidatePath(destFileName);
            File.Copy(sourceFileName, destFileName, overwrite);
        }
        public FileStream Create(string path)
        {
            path = ValidatePath(path);
            return File.Create(path);
        }
        public FileStream Create(string path, int bufferSize)
        {
            path = ValidatePath(path);
            return File.Create(path, bufferSize);
        }
        public FileStream Create(string path, int bufferSize, FileOptions options)
        {
            path = ValidatePath(path);
            return File.Create(path, bufferSize, options);
        }
        public void Delete(string path)
        {
            path = ValidatePath(path);
            File.Delete(path);
        }
        public bool Exists(string path)
        {
            path = ValidatePath(path);
            return File.Exists(path);
        }
        public FileStream Open(string path, FileMode mode)
        {
            path = ValidatePath(path);
            return File.Open(path, mode);
        }
        public FileStream Open(string path, FileMode mode, FileAccess access)
        {
            path = ValidatePath(path);
            return File.Open(path, mode, access);
        }
        public FileStream Open(string path, FileMode mode, FileAccess access, FileShare share)
        {
            path = ValidatePath(path);
            return File.Open(path, mode, access, share);
        }
        public void SetCreationTime(string path, DateTime creationTime)
        {
            path = ValidatePath(path);
            File.SetCreationTime(path, creationTime);
        }
        public void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
        {
            path = ValidatePath(path);
            File.SetCreationTimeUtc(path, creationTimeUtc);
        }
        public DateTime GetCreationTime(string path)
        {
            path = ValidatePath(path);
            return File.GetCreationTime(path);
        }
        public DateTime GetCreationTimeUtc(string path)
        {
            path = ValidatePath(path);
            return File.GetCreationTimeUtc(path);
        }
        public void SetLastAccessTime(string path, DateTime lastAccessTime)
        {
            path = ValidatePath(path);
            File.SetLastAccessTime(path, lastAccessTime);
        }
        public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
        {
            path = ValidatePath(path);
            File.SetLastAccessTimeUtc(path, lastAccessTimeUtc);
        }
        public DateTime GetLastAccessTime(string path)
        {
            path = ValidatePath(path);
            return File.GetLastAccessTime(path);
        }
        public DateTime GetLastAccessTimeUtc(string path)
        {
            path = ValidatePath(path);
            return File.GetLastAccessTimeUtc(path);
        }
        public void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            path = ValidatePath(path);
            File.SetLastWriteTime(path, lastWriteTime);
        }
        public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            path = ValidatePath(path);
            File.SetLastWriteTimeUtc(path, lastWriteTimeUtc);
        }
        public DateTime GetLastWriteTime(string path)
        {
            path = ValidatePath(path);
            return File.GetLastWriteTime(path);
        }
        public DateTime GetLastWriteTimeUtc(string path)
        {
            path = ValidatePath(path);
            return File.GetLastWriteTimeUtc(path);
        }
        public FileAttributes GetAttributes(string path)
        {
            path = ValidatePath(path);
            return File.GetAttributes(path);
        }
        public void SetAttributes(string path, FileAttributes fileAttributes)
        {
            path = ValidatePath(path);
            File.SetAttributes(path, fileAttributes);
        }
        public FileStream OpenRead(string path)
        {
            path = ValidatePath(path);
            return File.OpenRead(path);
        }
        public FileStream OpenWrite(string path)
        {
            path = ValidatePath(path);
            return File.OpenWrite(path);
        }
        public string ReadAllText(string path)
        {
            path = ValidatePath(path);
            return File.ReadAllText(path);
        }
        public string ReadAllText(string path, Encoding encoding)
        {
            path = ValidatePath(path);
            return File.ReadAllText(path, encoding);
        }
        public void WriteAllText(string path, string contents)
        {
            path = ValidatePath(path);
            File.WriteAllText(path, contents);
        }
        public void WriteAllText(string path, string contents, Encoding encoding)
        {
            path = ValidatePath(path);
            File.WriteAllText(path, contents, encoding);
        }
        public byte[] ReadAllBytes(string path)
        {
            path = ValidatePath(path);
            return File.ReadAllBytes(path);
        }
        public void WriteAllBytes(string path, byte[] bytes)
        {
            path = ValidatePath(path);
            File.WriteAllBytes(path, bytes);
        }
        public string[] ReadAllLines(string path)
        {
            path = ValidatePath(path);
            return File.ReadAllLines(path);
        }
        public string[] ReadAllLines(string path, Encoding encoding)
        {
            path = ValidatePath(path);
            return File.ReadAllLines(path, encoding);
        }
        public IEnumerable<string> ReadLines(string path)
        {
            path = ValidatePath(path);
            return File.ReadLines(path);
        }
        public IEnumerable<string> ReadLines(string path, Encoding encoding)
        {
            path = ValidatePath(path);
            return File.ReadLines(path, encoding);
        }
        public void WriteAllLines(string path, string[] contents)
        {
            path = ValidatePath(path);
            File.WriteAllLines(path, contents);
        }
        public void WriteAllLines(string path, IEnumerable<string> contents)
        {
            path = ValidatePath(path);
            File.WriteAllLines(path, contents);
        }
        public void WriteAllLines(string path, string[] contents, Encoding encoding)
        {
            path = ValidatePath(path);
            File.WriteAllLines(path, contents);
        }
        public void WriteAllLines(string path, IEnumerable<string> contents, Encoding encoding)
        {
            path = ValidatePath(path);
            File.WriteAllLines(path, contents, encoding);
        }
        public void AppendAllText(string path, string contents)
        {
            path = ValidatePath(path);
            File.AppendAllText(path, contents);
        }
        public void AppendAllText(string path, string contents, Encoding encoding)
        {
            path = ValidatePath(path);
            File.AppendAllText(path, contents);
        }
        public void AppendAllLines(string path, IEnumerable<string> contents)
        {
            path = ValidatePath(path);
            File.AppendAllLines(path, contents);
        }
        public void AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding)
        {
            path = ValidatePath(path);
            File.AppendAllLines(path, contents, encoding);
        }
        public void Replace(string sourceFileName, string destinationFileName, string destinationBackupFileName)
        {
            sourceFileName = ValidatePath(sourceFileName);
            destinationFileName = ValidatePath(destinationFileName);
            File.Replace(sourceFileName, destinationFileName, destinationBackupFileName);
        }
        public void Replace(string sourceFileName, string destinationFileName, string destinationBackupFileName, bool ignoreMetadataErrors)
        {
            sourceFileName = ValidatePath(sourceFileName);
            destinationFileName = ValidatePath(destinationFileName);
            File.Replace(sourceFileName, destinationFileName, destinationBackupFileName, ignoreMetadataErrors);
        }
        public void Move(string sourceFileName, string destFileName)
        {
            sourceFileName = ValidatePath(sourceFileName);
            destFileName = ValidatePath(destFileName);
            File.Move(sourceFileName, destFileName);
        }
        public void Move(string sourceFileName, string destFileName, bool overwrite)
        {
            sourceFileName = ValidatePath(sourceFileName);
            destFileName = ValidatePath(destFileName);
            File.Move(sourceFileName, destFileName, overwrite);
        }
        public void Encrypt(string path)
        {
            path = ValidatePath(path);
            if (OperatingSystem.IsWindows())
            {
                File.Encrypt(path);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public void Decrypt(string path)
        {
            path = ValidatePath(path);
            if (OperatingSystem.IsWindows())
            {
                File.Decrypt(path);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
        {
            path = ValidatePath(path);
            return File.ReadAllTextAsync(path, cancellationToken);
        }
        public Task<string> ReadAllTextAsync(string path, Encoding encoding, CancellationToken cancellationToken = default)
        {
            path = ValidatePath(path);
            return File.ReadAllTextAsync(path, encoding, cancellationToken);
        }
        public Task WriteAllTextAsync(string path, string contents, CancellationToken cancellationToken = default)
        {
            path = ValidatePath(path);
            return File.WriteAllTextAsync(path, contents, cancellationToken);
        }
        public Task WriteAllTextAsync(string path, string contents, Encoding encoding, CancellationToken cancellationToken = default)
        {
            path = ValidatePath(path);
            return File.WriteAllTextAsync(path, contents, encoding, cancellationToken);
        }
        public Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
        {
            path = ValidatePath(path);
            return File.ReadAllBytesAsync(path, cancellationToken);
        }
        public Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
        {
            path = ValidatePath(path);
            return File.WriteAllBytesAsync(path, bytes, cancellationToken);
        }
        public Task<string[]> ReadAllLinesAsync(string path, CancellationToken cancellationToken = default)
        {
            path = ValidatePath(path);
            return File.ReadAllLinesAsync(path, cancellationToken);
        }
        public Task<string[]> ReadAllLinesAsync(string path, Encoding encoding, CancellationToken cancellationToken = default)
        {
            path = ValidatePath(path);
            return File.ReadAllLinesAsync(path, encoding, cancellationToken);
        }
        public Task WriteAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken = default)
        {
            path = ValidatePath(path);
            return File.WriteAllLinesAsync(path, contents, cancellationToken);
        }
        public Task WriteAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default)
        {
            path = ValidatePath(path);
            return File.WriteAllLinesAsync(path, contents, encoding, cancellationToken);
        }
        public Task AppendAllTextAsync(string path, string contents, CancellationToken cancellationToken = default)
        {
            path = ValidatePath(path);
            return File.AppendAllTextAsync(path, contents, cancellationToken);
        }
        public Task AppendAllTextAsync(string path, string contents, Encoding encoding, CancellationToken cancellationToken = default)
        {
            path = ValidatePath(path);
            return File.AppendAllTextAsync(path, contents, encoding, cancellationToken);
        }
        public Task AppendAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken = default)
        {
            path = ValidatePath(path);
            return File.AppendAllLinesAsync(path, contents, cancellationToken);
        }
        public Task AppendAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default)
        {
            path = ValidatePath(path);
            return File.AppendAllLinesAsync(path, contents, encoding, cancellationToken);
        }

        private string ValidatePath(string path)
        {
            if (!string.IsNullOrWhiteSpace(ContainerName))
            {
                path = Path.Combine(ContainerName, path);
            }

            if (CreateIfNotExists)
            {
                var targetDirectory = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(targetDirectory) && !Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }
            }

            return path;
        }

        #region Extended

        public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
        {
            path = ValidatePath(path);
            Delete(path);
            return Task.CompletedTask;
        }

        public Task<Stream> ReadStreamAsync(string path, CancellationToken cancellationToken = default)
            => InternalReadStreamAsync(path, cancellationToken);

        // ReSharper disable once UnusedParameter.Local
        private Task<Stream> InternalReadStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            path = ValidatePath(path);
            Stream stream = File.Open(path, FileMode.Open);
            return Task.FromResult(stream);
        }

        public Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
            => InternalExistsAsync(path, cancellationToken);

        // ReSharper disable once UnusedParameter.Local
        private Task<bool> InternalExistsAsync(string path, CancellationToken cancellationToken = default)
        {
            path = ValidatePath(path);
            return Task.FromResult(File.Exists(path));
        }

        #endregion

        #endregion

        #region IDirectoryService

        public DirectoryInfo GetParent(string path) => Directory.GetParent(path);
        public DirectoryInfo CreateDirectory(string path) => Directory.CreateDirectory(path);
        public string[] GetFiles(string path) => Directory.GetFiles(path);
        public string[] GetFiles(string path, string searchPattern) => Directory.GetFiles(path, searchPattern);
        public string[] GetFiles(string path, string searchPattern, SearchOption searchOption) => Directory.GetFiles(path, searchPattern, searchOption);
        public string[] GetFiles(string path, string searchPattern, EnumerationOptions enumerationOptions) => Directory.GetFiles(path, searchPattern, enumerationOptions);
        public string[] GetDirectories(string path) => Directory.GetDirectories(path);
        public string[] GetDirectories(string path, string searchPattern) => Directory.GetDirectories(path, searchPattern);
        public string[] GetDirectories(string path, string searchPattern, SearchOption searchOption) => Directory.GetDirectories(path, searchPattern, searchOption);
        public string[] GetDirectories(string path, string searchPattern, EnumerationOptions enumerationOptions) => Directory.GetDirectories(path, searchPattern, enumerationOptions);
        public string[] GetFileSystemEntries(string path) => Directory.GetFileSystemEntries(path);
        public string[] GetFileSystemEntries(string path, string searchPattern) => Directory.GetFileSystemEntries(path, searchPattern);
        public string[] GetFileSystemEntries(string path, string searchPattern, SearchOption searchOption) => Directory.GetFileSystemEntries(path, searchPattern, searchOption);
        public string[] GetFileSystemEntries(string path, string searchPattern, EnumerationOptions enumerationOptions) => Directory.GetFileSystemEntries(path, searchPattern, enumerationOptions);
        public IEnumerable<string> EnumerateDirectories(string path) => Directory.EnumerateDirectories(path);
        public IEnumerable<string> EnumerateDirectories(string path, string searchPattern) => Directory.EnumerateDirectories(path, searchPattern);
        public IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption) => Directory.EnumerateDirectories(path, searchPattern, searchOption);
        public IEnumerable<string> EnumerateDirectories(string path, string searchPattern, EnumerationOptions enumerationOptions) => Directory.EnumerateDirectories(path, searchPattern, enumerationOptions);
        public IEnumerable<string> EnumerateFiles(string path) => Directory.EnumerateFiles(path);
        public IEnumerable<string> EnumerateFiles(string path, string searchPattern) => Directory.EnumerateFiles(path, searchPattern);
        public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption) => Directory.EnumerateFiles(path, searchPattern, searchOption);
        public IEnumerable<string> EnumerateFiles(string path, string searchPattern, EnumerationOptions enumerationOptions) => Directory.EnumerateFiles(path, searchPattern, enumerationOptions);
        public IEnumerable<string> EnumerateFileSystemEntries(string path) => Directory.EnumerateFileSystemEntries(path);
        public IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern) => Directory.EnumerateFileSystemEntries(path, searchPattern);
        public IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, SearchOption searchOption) => Directory.EnumerateFileSystemEntries(path, searchPattern, searchOption);
        public IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, EnumerationOptions enumerationOptions) => Directory.EnumerateFileSystemEntries(path, searchPattern, enumerationOptions);
        public string GetDirectoryRoot(string path) => Directory.GetDirectoryRoot(path);
        public string GetCurrentDirectory() => Directory.GetCurrentDirectory();
        public void SetCurrentDirectory(string path) { Directory.SetCurrentDirectory(path); }
        public void Delete(string path, bool recursive) => Directory.Delete(path, recursive);
        public string[] GetLogicalDrives() => Directory.GetLogicalDrives();
        public FileSystemInfo CreateSymbolicLink(string path, string pathToTarget) => Directory.CreateSymbolicLink(path, pathToTarget);
        public FileSystemInfo ResolveLinkTarget(string linkPath, bool returnFinalTarget) => Directory.ResolveLinkTarget(linkPath, returnFinalTarget);

        #endregion

        #region URL

        public Task<string> GetUrlAsync(string path, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
        public string GetUrl(string path)
        {
            throw new NotImplementedException();
        }

        #endregion

        public Task<string[]> GetFilesAsync(string path, string searchPattern = ".*", SearchOption searchOption = SearchOption.TopDirectoryOnly, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<FileRecord[]> GetFilesInfoAsync(string path, string searchPattern = ".*", SearchOption searchOption = SearchOption.TopDirectoryOnly, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<FileRecord> GetFileInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
