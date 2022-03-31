using System;
using System.Collections.Generic;
using Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities.Identity
{
    public class Role : IdentityRole<int>, IEntity<int>, IAuditable
    {
        public Role()
        {
            CreatedAt = DateTime.Now;
            RoleClaims = new List<RoleClaim>();
            UserRoles = new List<UserRole>();
        }

		public Role(string name, string description = "") : this()
		{
			base.Name = name;
			Description = description;
		}

        public string Description { get; set; }

        public ICollection<RoleClaim> RoleClaims { get; set; }

        public ICollection<UserRole> UserRoles { get; set; }

        #region IAuditable

        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        #endregion IAuditable

    }
}
