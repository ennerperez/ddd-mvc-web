using System;
using Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities.Identity
{
    public class UserToken : IdentityUserToken<int>, IEntity, IAuditable
    {
        public UserToken()
        {
            CreatedAt = DateTime.Now;
        }

        public virtual User User { get; set; }

        public int Id { get; set; }

        #region IAuditable

        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        #endregion IAuditable
    }
}
