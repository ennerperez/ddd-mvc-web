using System;
using System.Collections.Generic;
using Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities.Identity
{
    public class User : IdentityUser<int>, IEntity<int>, IAuditable
    {
        public User()
        {
            CreatedAt = DateTime.Now;
            UserRoles = new List<UserRole>();
            UserLogins = new List<UserLogin>();
            UserTokens = new List<UserToken>();
            UserClaims = new List<UserClaim>();
        }
        
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public string FullName => string.Join(" ", GivenName, Surname);
        
        public ICollection<UserRole> UserRoles { get; set; }
        public ICollection<UserLogin> UserLogins { get; set; }
        public ICollection<UserToken> UserTokens { get; set; }
        public ICollection<UserClaim> UserClaims { get; set; }

        #region IAuditable

        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        #endregion IAuditable

    }
}
