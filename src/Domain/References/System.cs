using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace System
{
    public static class EnumExtensions
    {
        // This extension method is broken out so you can use a similar pattern with
        // other MetaData elements in the future. This is your base method for each.
        public static T GetAttribute<T>(this Enum value) where T : Attribute
        {
            var type = value.GetType();
            var memberInfo = type.GetMember(value.ToString());
            var attributes = memberInfo[0].GetCustomAttributes(typeof(T), false);
            return attributes.Length > 0
                ? (T)attributes[0]
                : null;
        }

        // This method creates a specific call to the above method, requesting the
        // Description MetaData attribute.
        public static string ToName(this Enum value)
        {
            var attribute = value.GetAttribute<DescriptionAttribute>();
            return attribute == null ? value.ToString() : attribute.Description;
        }

        public static Dictionary<TValue, string> ToDictionary<TValue>(this Type type) where TValue : struct, IComparable<TValue>, IEquatable<TValue>
        {
            if (!type.IsEnum)
                throw new ArgumentException("Type must be an enum");

            var result = new Dictionary<TValue, string>();
            var values = Enum.GetValues(type);

            foreach (var item in values)
            {
                var memInfo = type.GetMember(type.GetEnumName(item) ?? string.Empty);
                var description = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;

                if (description != null)
                    result.Add((TValue)item, description.Description);
            }

            return result;
        }

        public static Dictionary<short, string> ToDictionary<TEnum>() where TEnum : Enum
            => ToDictionary<TEnum, short>();

        public static Dictionary<TValue, string> ToDictionary<TEnum, TValue>() where TEnum : Enum where TValue : struct, IComparable<TValue>, IEquatable<TValue>
            => typeof(TEnum).ToDictionary<TValue>();
    }
}
