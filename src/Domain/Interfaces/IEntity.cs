using System;

namespace Domain.Interfaces
{
    public interface IEntity : IEntity<int>;

    public interface IEntity<TKey> where TKey : struct, IComparable<TKey>
    {
        public TKey Id { get; set; }
    }
}
