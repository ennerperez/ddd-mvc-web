using System;
using Domain.Interfaces;

namespace Domain.Abstractions
{
    public abstract class Entity : Entity<long>
    {
        public override long Id { get; set; }
    }

    public abstract class Entity<TKey> : IEntity<TKey> where TKey : struct, IEquatable<TKey>, IComparable<TKey>
    {
        public virtual TKey Id { get; set; }

        public abstract override string ToString();

        public override bool Equals(object obj) => obj != null && obj is Entity<TKey> entity && Id.Equals(entity.Id);

        protected bool Equals(Entity<TKey> other) => Id.Equals(other.Id);

        public override int GetHashCode() =>
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            Id.GetHashCode();
    }
}
