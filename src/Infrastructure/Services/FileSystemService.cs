﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Interfaces;

namespace Infrastructure.Services
{
	public class FileSystemService : IFileService
	{

		public string ContainerName { get; set; }
		public bool CreateIfNotExists { get; set; }
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
				File.Encrypt(path);
			else
				throw new NotImplementedException();
		}
		public void Decrypt(string path)
		{
			path = ValidatePath(path);
			if (OperatingSystem.IsWindows())
				File.Decrypt(path);
			else
				throw new NotImplementedException();
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
				path = Path.Combine(ContainerName, path);
			if (CreateIfNotExists)
			{
				var targetDirectory = Path.GetDirectoryName(path);
				if (!string.IsNullOrWhiteSpace(targetDirectory) && !Directory.Exists(targetDirectory))
					Directory.CreateDirectory(targetDirectory);
			}
			return path;
		}
	}
}
