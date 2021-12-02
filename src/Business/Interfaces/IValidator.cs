using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Domain.Interfaces;

namespace Business.Interfaces
{
    public interface IValidator<TEntity> : IValidator<TEntity, int> where TEntity : class, IEntity<int>
    {
    }

    public interface IValidator<TEntity, TKey> where TEntity : class, IEntity<TKey> where TKey : struct, IComparable<TKey>, IEquatable<TKey>
    {
        Task ValidateAsync(TEntity entity, [CallerMemberName] string callerMemberName = "");
    }
}
