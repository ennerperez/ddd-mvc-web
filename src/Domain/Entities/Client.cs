using System;
using System.Collections.Generic;
using Domain.Interfaces;

namespace Domain.Entities
{
    public class Client : IEntity, IAuditable
    {
        public Client()
        {
            CreatedAt = DateTime.Now;
        }

        public string Identification { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Category { get; set; }

        public ICollection<Budget> Budgets { get; set; }
        public int Id { get; set; }

        #region IAuditable

        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        #endregion
    }
}
