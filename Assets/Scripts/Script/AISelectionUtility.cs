using System;
using System.Collections.Generic;
using System.Linq;

// Pure selection logic (no UnityEngine references) so it can be unit-tested
// without a running Unity instance.
public static class AISelectionUtility
{
    public const int MaxRandomAttempts = 200;
    public const int MaxExhaustiveCombinations = 60000;

    // Returns the chosen indexes, or null when the AI should decline (no selection).
    public static List<int> Choose(
        int totalCount,                         // number of eligible candidates (indexes 0..totalCount-1)
        Func<List<int>, bool> canEndSelect,     // validates a complete selection (e.g. total-cost cap)
        Func<List<int>, int, bool> canAdd,      // incremental validation as the selection grows (nullable)
        int maxCount,                           // maximum selectable targets
        bool canNoSelect,                       // declining (returning null) is allowed
        bool canEndNotMax)                      // ending with fewer than maxCount is allowed
    {
        if (totalCount <= 0)
        {
            return null;
        }

        // Try from the largest possible selection down to the smallest.
        for (int selectCount = Math.Min(maxCount, totalCount); selectCount >= 1; selectCount--)
        {
            // Only sizes the ending rule allows may be sent.
            if (!(selectCount == maxCount || (selectCount <= maxCount && canEndNotMax)))
            {
                continue;
            }

            // Random sampling.
            for (int attempt = 0; attempt < MaxRandomAttempts; attempt++)
            {
                List<int> candidate = RandomSample(totalCount, selectCount);

                if (IsValid(candidate, canEndSelect, canAdd))
                {
                    return candidate;
                }
            }

            // Exhaustive scan as a fallback.
            foreach (List<int> candidate in EnumerateCombinations(totalCount, selectCount, MaxExhaustiveCombinations))
            {
                if (IsValid(candidate, canEndSelect, canAdd))
                {
                    return candidate;
                }
            }
        }

        if (canNoSelect)
        {
            return null;
        }

        // Last resort: pick the first target rather than hanging.
        return new List<int>() { 0 };
    }

    static bool IsValid(List<int> candidate, Func<List<int>, bool> canEndSelect, Func<List<int>, int, bool> canAdd)
    {
        if (canEndSelect != null)
        {
            if (!canEndSelect(candidate))
            {
                return false;
            }
        }

        if (canAdd != null)
        {
            List<int> prefix = new List<int>();

            foreach (int index in candidate)
            {
                if (!canAdd(prefix, index))
                {
                    return false;
                }

                prefix.Add(index);
            }
        }

        return true;
    }

    static List<int> RandomSample(int totalCount, int count)
    {
        Random random = new Random();
        List<int> pool = Enumerable.Range(0, totalCount).ToList();
        List<int> result = new List<int>();

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int draw = random.Next(0, pool.Count);
            result.Add(pool[draw]);
            pool.RemoveAt(draw);
        }

        return result;
    }

    // Enumerates C(n,k) index combinations, yielding at most maxResults.
    static IEnumerable<List<int>> EnumerateCombinations(int n, int k, int maxResults)
    {
        int[] indexes = new int[k];
        int yielded = 0;

        return Enumerate(0, 0);

        IEnumerable<List<int>> Enumerate(int start, int depth)
        {
            if (yielded >= maxResults)
            {
                yield break;
            }

            if (depth == k)
            {
                yielded++;
                yield return indexes.ToList();
                yield break;
            }

            for (int i = start; i < n; i++)
            {
                indexes[depth] = i;

                foreach (List<int> combo in Enumerate(i + 1, depth + 1))
                {
                    yield return combo;
                }
            }
        }
    }
}