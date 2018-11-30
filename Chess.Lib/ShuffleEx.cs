using System;
using System.Collections.Generic;
using System.Linq;

namespace Chess.Lib
{
    public static class SuffleEx
    {
        #region Members

        private static readonly Random _random = new Random();

        #endregion Members

        #region Methods

        /// <summary>
        /// Retrieve the given list as a linear shuffled list containing the same elements as before.
        /// </summary>
        /// <typeparam name="T">the element type of the list</typeparam>
        /// <param name="items">the list containing the elements</param>
        /// <returns>a new list with the same elements in a different order</returns>
        public static List<T> Shuffle<T>(this IEnumerable<T> items)
        {
            // make sure the overloaded list is not null
            if (items == null) { throw new ArgumentException("items must not be null"); }

            // fix the order of the given elements by converting the enumerable to a list
            var results = items.ToList();

            // shuffle all elements (=> linear shuffle with equal distribution)
            for (int i = 0; i < results.Count - 1; i++)
            {
                // get index to switch with
                int k = _random.Next(i, results.Count);

                // check if element needs to switch (same index => avoid switching with itself)
                if (i != k)
                {
                    // switch position: results[k] <--> results[i]
                    T value = results[k];
                    results[k] = results[i];
                    results[i] = value;
                }
            }

            return results;
        }

        /// <summary>
        /// Retrieve a randomly selected element from the given list.
        /// </summary>
        /// <typeparam name="T">the element type of the list</typeparam>
        /// <param name="list">the list containing the elements</param>
        /// <returns>a randomly selected element</returns>
        public static T ChooseRandom<T>(this IEnumerable<T> list)
        {
            // get the count of the list safely
            int count = list != null ? Enumerable.Count(list) : 0;
            if (count == 0) { throw new ArgumentException("list must not be null or empty"); }

            // determine the selected element
            int index = _random.Next(0, count);
            return Enumerable.ElementAt(list, index);
        }

        /// <summary>
        /// Retrieve a subset of the given size containing randomly selected elements from the given list.
        /// </summary>
        /// <typeparam name="T">the element type of the list</typeparam>
        /// <param name="list">the list containing the elements</param>
        /// <param name="range">the number of elements to select</param>
        /// <returns>a subset of the given list</returns>
        public static List<T> ChooseRandom<T>(this IEnumerable<T> list, int range)
        {
            // get the count of the list safely
            int count = list != null ? Enumerable.Count(list) : 0;
            if (count == 0) { throw new ArgumentException("list must not be null or empty"); }

            // determine the selected elements
            return list.Shuffle().Take(range > count ? count : range).ToList();
        }

        #endregion Methods
    }
}
