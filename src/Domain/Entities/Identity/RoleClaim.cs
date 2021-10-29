using Microsoft.AspNetCore.Identity;
using System;
using Domain.Interfaces;

namespace Domain.Entities
{
    public class RoleClaim : IdentityRoleClaim<int>, IEntity<int>, IAuditable
    {
        public RoleClaim()
        {
            CreatedAt = DateTime.Now;
        }

        public virtual Role Role { get; set; }

        #region IAuditable

        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        #endregion IAuditable
    }
}
