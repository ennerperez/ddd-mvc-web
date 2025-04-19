using System;
#if USING_IDENTITY
using Domain.Entities.Identity;
#endif

namespace Domain.Interfaces
{
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
        DateTime? DeletedAt { get; set; }
    }

    public interface IExtendedSoftDelete<TKey> where TKey : struct, IComparable<TKey>, IEquatable<TKey>
    {
        TKey? DeletedById { get; set; }
#if USING_IDENTITY
        User DeletedBy { get; set; }
#endif
    }

    public interface IExtendedSoftDelete : IExtendedSoftDelete<int>
    {
    }
}
