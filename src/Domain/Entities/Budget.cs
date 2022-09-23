using System;
using Domain.Entities.Identity;
using Domain.Enums;
using Domain.Interfaces;

namespace Domain.Entities
{
	public class Budget : IEntity<Guid>, IAuditable, IExtendedAuditable, ISoftDelete, IExtendedSoftDelete
	{
		public Budget()
		{
			CreatedAt = DateTime.Now;
			State = States.Draft;
		}
		public Guid Id { get; set; }

		public string Code { get; set; }
		
		public int? ClientId { get; set; }
		public Client Client { get; set; }

		public States State { get; set; }

		public decimal? Subtotal { get; set; }
		public decimal? Taxes { get; set; }
		public decimal? Total { get; set; }
		
		public DateTime? ExpireAt { get; set; }
		public bool IsDeleted { get; set; }
		
		public int? CreatedById { get; set; }
		public User CreatedBy { get; set; }
		public DateTime CreatedAt { get; set; }
		
		public int? ModifiedById { get; set; }
		public User ModifiedBy { get; set; }
		public DateTime? ModifiedAt { get; set; }
		
		public User DeletedBy { get; set; }
		public int? DeletedById { get; set; }
		public DateTime? DeletedAt { get; set; }
		
	}
}
