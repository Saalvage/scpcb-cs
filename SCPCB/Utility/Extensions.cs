using System.Numerics;

namespace SCPCB.Utility; 

public static class Extensions {
    public static IEnumerable<T> AsEnumerableElementOrEmpty<T>(this T? item) {
        if (item != null) {
            yield return item;
        }
    }

    public static void AddRange<T>(this IList<T> list, IEnumerable<T> range) {
        foreach (var item in range) {
            list.Add(item);
        }
    }

    public static T RandomElement<T>(this IEnumerable<T> enumerable)
        => enumerable.ElementAt(Random.Shared.Next(enumerable.Count()));

    public static IEnumerable<T> CumSum<T>(this IEnumerable<T> enumerable) where T : struct, IAdditionOperators<T, T, T> {
        T val = default;
        foreach (var item in enumerable) {
            val += item;
            yield return val;
        }
    }

    public static IEnumerable<(T Item, TSum CumSum)> CumSumBy<T, TSum>(this IEnumerable<T> enumerable, Func<T, TSum> by)
        where TSum : struct, IAdditionOperators<TSum, TSum, TSum>
        => enumerable.CumSumBy(by, (item, val) => (item, val));

    public static IEnumerable<TRes> CumSumBy<T, TSum, TRes>(this IEnumerable<T> enumerable, Func<T, TSum> by, Func<T, TSum, TRes> selector)
        where TSum : struct, IAdditionOperators<TSum, TSum, TSum> {
        TSum val = default;
        foreach (var item in enumerable) {
            val += by(item);
            yield return selector(item, val);
        }
    }

    public static int BinarySearch<T, TVal>(this IReadOnlyList<T> list, TVal value, int start, int length, Func<T, TVal, int> comparer) {
        var low = start;
        var high = start + length - 1;
        while (low <= high) {
            var mid = low + (high - low) / 2;
            var comp = comparer(list[mid], value);
            if (comp == 0) {
                return mid;
            } else if (comp < 0) {
                low = mid + 1;
            } else {
                high = mid - 1;
            }
        }
        return ~low;
    }

    public static int BinarySearch<T, TVal>(this IReadOnlyList<T> list, TVal value, Func<T, TVal, int> comparer)
        => list.BinarySearch(value, 0, list.Count, comparer);
}
