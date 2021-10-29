using System;

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
}
