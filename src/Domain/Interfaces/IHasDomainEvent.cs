using System.Collections.Generic;
using Domain.Abstractions;

namespace Domain.Interfaces
{
    public interface IHasDomainEvent : IHasDomainEvent<IDomainEvent>
    {
    }

    public interface IHasDomainEvent<T> where T : class
    {
        IList<DomainEvent<T>> DomainEvents { get; set; }
    }
}
