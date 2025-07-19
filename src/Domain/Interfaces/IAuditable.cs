using System;
#if USING_IDENTITY
using Domain.Entities.Identity;
#endif

namespace Domain.Interfaces
{
    public interface IAuditable
    {
        DateTime? CreatedAt { get; set; }
        DateTime? ModifiedAt { get; set; }
    }

    public interface IExtendedAuditable<TKey> where TKey : struct, IComparable<TKey>, IEquatable<TKey>
    {
        TKey? CreatedById { get; set; }
        TKey? ModifiedById { get; set; }
#if USING_IDENTITY
        User CreatedBy { get; set; }
        User ModifiedBy { get; set; }
#endif
    }

    public interface IExtendedAuditable : IExtendedAuditable<int>
    {
    }
}
