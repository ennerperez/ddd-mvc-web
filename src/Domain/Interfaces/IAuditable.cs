using System;
using Domain.Entities.Identity;

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

    public interface IExtendedAuditable
    {
        int? CreatedById { get; set; }
        User CreatedBy { get; set; }
        
        int? ModifiedById { get; set; }
        User ModifiedBy { get; set; }
    }
}
