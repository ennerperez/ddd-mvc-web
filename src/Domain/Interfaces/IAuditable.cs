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
        DateTime Created { get; set; }

        /// <summary>
        /// Last modification datetime
        /// </summary>
        DateTime? Modified { get; set; }
    }
}
