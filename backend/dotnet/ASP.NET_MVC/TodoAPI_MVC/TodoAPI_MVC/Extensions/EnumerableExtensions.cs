using System.Diagnostics.CodeAnalysis;

namespace TodoAPI_MVC.Extensions
{
    public static class EnumerableExtensions
    {
        public static int GetNextValue<T>(
            this IEnumerable<T> source, Func<T, int> selector, int step = 1, int startFrom = 1)
        {
            if (!source.Any())
                return startFrom;

            return source.Select(selector).Max() + step;
        }

        public static bool TryGetValueAt<T>(
            this IList<T> source, int index, [NotNullWhen(true)] out T? value)
        {
            if (source.Count <= index)
            {
                value = default;
                return false;
            }

            value = source[index]!;
            return true;
        }
    }
}
