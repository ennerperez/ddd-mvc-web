using System;
using Domain.Interfaces;

namespace Domain.Abstractions
{
    public class DomainEvent<T> : IDomainEvent where T : class
    {
        public DomainEvent()
        {
            DateOccurred = DateTimeOffset.UtcNow;
        }
        public DomainEvent(string name, T item) : this()
        {
            Name = name;
            Item = item;
        }

        public string Name { get; }

        public T Item { get; }

        public bool IsPublished { get; set; }
        public DateTimeOffset DateOccurred { get; protected set; }
    }
}
