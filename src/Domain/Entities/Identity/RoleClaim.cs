using Microsoft.AspNetCore.Identity;
using System;
using Domain.Interfaces;

namespace Domain.Entities
{
    public class RoleClaim : IdentityRoleClaim<int>, IEntity<int>, IAuditable
    {
        public RoleClaim()
        {
            Created = DateTime.Now;
        }

        public virtual Role Role { get; set; }

        #region IAuditable

        public DateTime Created { get; set; }
        public DateTime? Modified { get; set; }

        #endregion IAuditable
    }
}
