using System;

namespace Infrastructure.Records
{
    public record FileRecord(string Name, DateTime CreationTime, DateTime? LastModified, string Extension, long? Length);
}
