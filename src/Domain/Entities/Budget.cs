using System;
using Domain.Abstractions;
using Domain.Enums;
using Domain.Interfaces;
#if USING_IDENTITY
using Domain.Entities.Identity;
#endif

namespace Domain.Entities
{
    public class Budget : Entity<Guid>, IAuditable, IExtendedAuditable, ISoftDelete, IExtendedSoftDelete
    {
        public Budget()
        {
            CreatedAt = DateTime.Now;
            Status = Status.Draft;
        }

        public string Code { get; set; }

        public int ClientId { get; set; }
        public virtual Client Client { get; set; }

        public Status Status { get; set; }

        public decimal Subtotal { get; set; }
        public decimal Taxes { get; set; }
        public decimal Total { get; set; }

        public DateTime? ExpireAt { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? ModifiedAt { get; set; }

        public int? CreatedById { get; set; }

        public int? ModifiedById { get; set; }

        public int? DeletedById { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

#if USING_IDENTITY
        public User CreatedBy { get; set; }
        public User ModifiedBy { get; set; }
        public User DeletedBy { get; set; }
#endif
    }
}
