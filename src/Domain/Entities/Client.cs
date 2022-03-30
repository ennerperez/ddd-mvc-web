﻿using System;
using System.Collections.Generic;
using Domain.Abstractions;
using Domain.Interfaces;

namespace Domain.Entities
{
    public class Client : IEntity<int>, IAuditable, IHasDomainEvent<Client>
    {

        public Client()
        {
            CreatedAt = DateTime.Now;
            DomainEvents = new List<DomainEvent<Client>>();
        }
        public int Id { get; set; }

        public string Identification { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Category { get; set; }
        
        #region IAuditable
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        #endregion

        public IList<DomainEvent<Client>> DomainEvents { get; set; }
    }
}
