using Domain.Interfaces;

#pragma warning disable CS8632

namespace System.Reflection
{
    public static class EntityExtensions
    {
        public static bool IsSoftDelete(this IEntity entity)
        {
            return typeof(ISoftDelete).IsAssignableFrom(entity.GetType());
        }
    }
}
