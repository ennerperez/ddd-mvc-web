using System;

namespace Domain.Interfaces
{
    public interface IDomainEvent
    {
        bool IsPublished { get; set; }
        DateTimeOffset DateOccurred { get; }
    }
}
