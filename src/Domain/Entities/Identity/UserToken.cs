using System;
using Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities.Identity
{
    public class UserToken : IdentityUserToken<int>, IEntity<int>, IAuditable
    {
        public UserToken()
        {
            CreatedAt = DateTime.Now;
        }

        public int Id { get; set; }

        public virtual User User { get; set; }

        #region IAuditable

        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        #endregion IAuditable
    }
}
