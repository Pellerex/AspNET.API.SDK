using System;
using System.Collections.Generic;
using System.Linq;

namespace ML.Utils
{
    internal static class StringExtension
    {
        public static IEnumerable<string> SplitAndKeep(
                              this string inputString, params char[] delimiters)
        {
            int start = 0, index;

            while ((index = inputString.IndexOfAny(delimiters, start)) != -1)
            {
                if (index - start > 0)
                    yield return inputString.Substring(start, index - start);

                yield return inputString.Substring(index, 1);

                start = index + 1;
            }

            if (start < inputString.Length)
            {
                yield return inputString.Substring(start);
            }
        }
    }

    public static class EnumerableExtension
    {
        public static IEnumerable<(T Item, float Probability)> Softmax<T>(
                                            this IEnumerable<T> collection,
                                            Func<T, float> scoreSelector)
        {
            var maxScore = collection.Max(scoreSelector);
            var sum = collection.Sum(r => Math.Exp(scoreSelector(r) - maxScore));

            return collection.Select(r => (r, (float)(Math.Exp(scoreSelector(r) - maxScore) / sum)));
        }
    }
}
