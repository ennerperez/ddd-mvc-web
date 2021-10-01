using Microsoft.AspNetCore.Identity;
using System;
using Domain.Interfaces;

namespace Domain.Entities
{
    public class UserLogin : IdentityUserLogin<int>, IEntity<int>, IAuditable
    {
        public UserLogin()
        {
            Created = DateTime.Now;
        }

        public int Id { get; set; }

        public virtual User User { get; set; }

        #region IAuditable

        public DateTime Created { get; set; }
        public DateTime? Modified { get; set; }

        #endregion IAuditable

    }
}
