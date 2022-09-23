using System;
using Domain.Entities.Identity;

namespace Domain.Interfaces
{
	public interface ISoftDelete
	{
		bool IsDeleted { get; set; }
		DateTime? DeletedAt { get; set; }
	}
	
	public interface IExtendedSoftDelete<TKey> where TKey : struct, IComparable<TKey>, IEquatable<TKey>
	{
		TKey? DeletedById { get; set; }
		User DeletedBy { get; set; }
	}
	
	public interface IExtendedSoftDelete : IExtendedSoftDelete<int>
	{
	}
}
