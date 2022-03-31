using System;
using Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities.Identity
{
    public sealed class UserClaim : IdentityUserClaim<int>, IEntity<int>, IAuditable
    {
        public UserClaim()
        {
            CreatedAt = DateTime.Now;
        }

        public UserClaim(string claimType, string claimValue) : this()
        {
            ClaimType = claimType;
            ClaimValue = claimValue;
        }

        public User User { get; set; }

        #region IAuditable

        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        #endregion IAuditable
    }
}
