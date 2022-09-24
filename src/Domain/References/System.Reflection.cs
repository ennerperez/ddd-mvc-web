﻿using Domain.Interfaces;

namespace System.Reflection
{
	public static class Extensions
	{
		public static bool IsSoftDelete(this IEntity entity)
		{
			return typeof(ISoftDelete).IsAssignableFrom(entity.GetType());
		}
	}
}
