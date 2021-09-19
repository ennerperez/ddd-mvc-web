using Microsoft.AspNetCore.Identity;
using System;
using Domain.Interfaces;

namespace Domain.Entities
{
    public sealed class UserClaim : IdentityUserClaim<int>, IEntity<int>, IAuditable
    {
        public UserClaim()
        {
            Created = DateTime.Now;
        }

        public UserClaim(string claimType, string claimValue) : this()
        {
            ClaimType = claimType;
            ClaimValue = claimValue;
        }

        public User User { get; set; }

        #region IAuditable

        public DateTime Created { get; set; }
        public DateTime? Modified { get; set; }

        #endregion IAuditable
    }
}
