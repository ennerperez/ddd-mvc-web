using Microsoft.AspNetCore.Identity;
using System;
using Domain.Interfaces;

namespace Domain.Entities
{
    public class UserRole : IdentityUserRole<int>, IEntity<int>, IAuditable
    {
        public UserRole()
        {
            Created = DateTime.Now;
        }

        public int Id { get; set; }

        public virtual User User { get; set; }

        public virtual Role Role { get; set; }

        #region IAuditable

        public DateTime Created { get; set; }
        public DateTime? Modified { get; set; }

        #endregion IAuditable
    }
}
