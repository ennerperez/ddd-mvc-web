using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using Domain.Interfaces;

namespace Domain.Entities
{
    public class Role : IdentityRole<int>, IEntity<int>, IAuditable
    {
        public Role()
        {
            Created = DateTime.Now;
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

        public DateTime Created { get; set; }
        public DateTime? Modified { get; set; }

        #endregion IAuditable

    }
}
