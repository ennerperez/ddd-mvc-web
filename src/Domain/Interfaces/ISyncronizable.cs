using System;

namespace Domain.Interfaces
{
    public interface ISyncronizable
    {
        Guid? RowKey { get; set; }

        byte[] RowVersion { get; set; }
    }
}
