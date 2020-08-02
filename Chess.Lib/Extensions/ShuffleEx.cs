/*
 * MIT License
 * 
 * Copyright(c) 2020 Marco Tröster
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Chess.Lib.Extensions
{
    /// <summary>
    /// This is an extension class that provides linear, equal distributed shuffle and random selection of elements from a given list. 
    /// Those operations can be used in by any enumerable.
    /// </summary>
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
        public static IList<T> Shuffle<T>(this IList<T> items)
        {
            // make sure the overloaded list is not null
            if (items == null) { throw new ArgumentException("items must not be null"); }

            IList<T> results;

            // make sure the overloaded list is not empty
            if (items?.Count > 0)
            {
                // fix the order of the given elements by converting the enumerable to a list
                var tempItems = items.ToArray();

                // shuffle all elements (=> linear shuffle with equal distribution)
                for (int i = 0; i < tempItems.Length - 1; i++)
                {
                    // get index to switch with
                    int k = _random.Next(i, tempItems.Length);

                    // check if element needs to switch (same index => avoid switching with itself)
                    if (i != k)
                    {
                        // switch position: results[k] <--> results[i]
                        T value = tempItems[k];
                        tempItems[k] = tempItems[i];
                        tempItems[i] = value;
                    }
                }

                results = tempItems.ToList();
            }
            else
            {
                results = new List<T>();
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
        public static IList<T> ChooseRandom<T>(this IList<T> list, int range)
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
