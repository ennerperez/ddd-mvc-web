using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using Domain.Interfaces;

namespace Domain.Entities
{
    public class User : IdentityUser<int>, IEntity<int>, IAuditable
    {
        public User()
        {
            Created = DateTime.Now;
            UserRoles = new List<UserRole>();
            UserLogins = new List<UserLogin>();
            UserTokens = new List<UserToken>();
            UserClaims = new List<UserClaim>();
        }

        public ICollection<UserRole> UserRoles { get; set; }
        public ICollection<UserLogin> UserLogins { get; set; }
        public ICollection<UserToken> UserTokens { get; set; }
        public ICollection<UserClaim> UserClaims { get; set; }

        #region IAuditable

        public DateTime Created { get; set; }
        public DateTime? Modified { get; set; }

        #endregion IAuditable
    }
}
