using System.Linq;

// ReSharper disable once CheckNamespace
namespace System.Collections
{
    namespace Generic
    {
        public static class Extensions
        {
            public static IEnumerable<List<T>> Split<T>(this List<T> source, int size = 30)
            {
                for (var i = 0; i < source.Count; i += size)
                    yield return source.GetRange(i, Math.Min(size, source.Count - i));
            }

            public static List<IEnumerable<T>> Split<T>(this IEnumerable<T> source, int parts = 30)
            {
                var data = source.ToArray();
                var result = new List<IEnumerable<T>>();
                var temp = new T[] { };
                var t = data.Length;
                var size = (int)Math.Ceiling(((decimal)t / parts));

                for (var i = 0; i < t; i++)
                {
                    temp = temp.Push(data[i]);
                    if (temp.Length >= size)
                    {
                        result.Add(temp);
                        temp = new T[] { };
                    }
                }

                if (temp.Any()) result.Add(temp);
                return result;
            }


            public static T[] Push<T>(this T[] target, T item)
            {
                if (target == null) return null;

                var result = new T[target.Length + 1];
                target.CopyTo(result, 0);
                result[target.Length] = item;
                return result;
            }
        }
    }
}
