#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Infrastructure.Interfaces
{
    public interface IDirectoryService
    {
        string ContainerName { get; set; }
        bool CreateIfNotExists { get; set; }
        string DirectoryExtension { get; set; }

        DirectoryInfo? GetParent(string path);
        DirectoryInfo CreateDirectory(string path);
        bool Exists([NotNullWhen(true)] string? path);
        void SetCreationTime(string path, DateTime creationTime);
        void SetCreationTimeUtc(string path, DateTime creationTimeUtc);
        DateTime GetCreationTime(string path);
        DateTime GetCreationTimeUtc(string path);
        void SetLastWriteTime(string path, DateTime lastWriteTime);
        void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc);
        DateTime GetLastWriteTime(string path);
        DateTime GetLastWriteTimeUtc(string path);
        void SetLastAccessTime(string path, DateTime lastAccessTime);
        void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc);
        DateTime GetLastAccessTime(string path);
        DateTime GetLastAccessTimeUtc(string path);
        string[] GetFiles(string path);
        string[] GetFiles(string path, string searchPattern);
        string[] GetFiles(string path, string searchPattern, SearchOption searchOption);
        string[] GetFiles(string path, string searchPattern, EnumerationOptions enumerationOptions);
        string[] GetDirectories(string path);
        string[] GetDirectories(string path, string searchPattern);
        string[] GetDirectories(string path, string searchPattern, SearchOption searchOption);
        string[] GetDirectories(string path, string searchPattern, EnumerationOptions enumerationOptions);
        string[] GetFileSystemEntries(string path);
        string[] GetFileSystemEntries(string path, string searchPattern);
        string[] GetFileSystemEntries(string path, string searchPattern, SearchOption searchOption);
        string[] GetFileSystemEntries(string path, string searchPattern, EnumerationOptions enumerationOptions);
        IEnumerable<string> EnumerateDirectories(string path);
        IEnumerable<string> EnumerateDirectories(string path, string searchPattern);
        IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption);
        IEnumerable<string> EnumerateDirectories(string path, string searchPattern, EnumerationOptions enumerationOptions);
        IEnumerable<string> EnumerateFiles(string path);
        IEnumerable<string> EnumerateFiles(string path, string searchPattern);
        IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption);
        IEnumerable<string> EnumerateFiles(string path, string searchPattern, EnumerationOptions enumerationOptions);
        IEnumerable<string> EnumerateFileSystemEntries(string path);
        IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern);
        IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, SearchOption searchOption);
        IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, EnumerationOptions enumerationOptions);
        string GetDirectoryRoot(string path);
        string GetCurrentDirectory();
        void SetCurrentDirectory(string path);
        void Move(string sourceDirName, string destDirName);
        void Delete(string path);
        void Delete(string path, bool recursive);
        string[] GetLogicalDrives();
        FileSystemInfo CreateSymbolicLink(string path, string pathToTarget);
        FileSystemInfo? ResolveLinkTarget(string linkPath, bool returnFinalTarget);
    }
}
