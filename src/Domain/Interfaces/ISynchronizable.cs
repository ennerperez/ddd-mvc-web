using System;

namespace Domain.Interfaces
{
    // ReSharper disable once IdentifierTypo
    public interface ISynchronizable
    {
        Guid? RowKey { get; set; }

        byte[] RowVersion { get; set; }
    }
}
