using System;
using Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities.Identity
{
    public class UserRole : IdentityUserRole<int>, IEntity, IAuditable
    {
        public UserRole()
        {
            CreatedAt = DateTime.Now;
        }

        public virtual User User { get; set; }

        public virtual Role Role { get; set; }

        public int Id { get; set; }

        #region IAuditable

        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        #endregion IAuditable
    }
}
