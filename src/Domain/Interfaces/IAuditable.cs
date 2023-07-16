using System;
#if USING_IDENTITY
using Domain.Entities.Identity;
#endif

namespace Domain.Interfaces
{
    /// <summary>
    /// Auditable structure for entities
    /// </summary>
    public interface IAuditable
    {
        /// <summary>
        /// Created datetime
        /// </summary>
        DateTime CreatedAt { get; set; }

        /// <summary>
        /// Last modification datetime
        /// </summary>
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
