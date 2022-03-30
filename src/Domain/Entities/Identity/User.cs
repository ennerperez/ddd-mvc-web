﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using Domain.Abstractions;
using Domain.Interfaces;

namespace Domain.Entities
{
    public class User : IdentityUser<int>, IEntity<int>, IAuditable, IHasDomainEvent<User>
    {
        public User()
        {
            CreatedAt = DateTime.Now;
            UserRoles = new List<UserRole>();
            UserLogins = new List<UserLogin>();
            UserTokens = new List<UserToken>();
            UserClaims = new List<UserClaim>();
            DomainEvents = new List<DomainEvent<User>>();
        }
        
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => string.Join(" ", FirstName, LastName);
        
        public ICollection<UserRole> UserRoles { get; set; }
        public ICollection<UserLogin> UserLogins { get; set; }
        public ICollection<UserToken> UserTokens { get; set; }
        public ICollection<UserClaim> UserClaims { get; set; }

        #region IAuditable

        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        #endregion IAuditable

        public IList<DomainEvent<User>> DomainEvents { get; set; }
    }
}
