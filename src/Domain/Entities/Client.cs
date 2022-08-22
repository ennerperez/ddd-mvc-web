using System;
using Domain.Interfaces;

namespace Domain.Entities
{
    public class Client : IEntity, IAuditable
    {

        public Client()
        {
            CreatedAt = DateTime.Now;
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

    }
}
