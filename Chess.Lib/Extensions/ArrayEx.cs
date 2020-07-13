using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Lib.Extensions
{
    /// <summary>
    /// An extension class for efficient array operations.
    /// </summary>
    public static class ArrayEx
    {
        #region Methods

        // TODO: check if this is really efficient due to generic types

        /// <summary>
        /// Concatenate the two given arrays efficiently.
        /// </summary>
        /// <typeparam name="T">The type of the arrays.</typeparam>
        /// <param name="first">The first array to concatenate.</param>
        /// <param name="second">The second array to concatenate.</param>
        /// <returns>a new array with concatenated array contents of the two given arrays.</returns>
        public static T[] ArrayConcat<T>(this T[] first, T[] second)
        {
            // TODO: check if empty arrays are concatenated correctly

            var ret = new T[first.Length + second.Length];
            first.CopyTo(ret, 0);
            second.CopyTo(ret, first.Length);
            return ret;
        }

        /// <summary>
        /// Cut parts from the given array of the given start index and length and apply them to a new array.
        /// </summary>
        /// <typeparam name="T">The type of the array.</typeparam>
        /// <param name="input">The input array to be cut.</param>
        /// <param name="index">The start index of the array content to be cut.</param>
        /// <param name="length">The length of the array content to be cut.</param>
        /// <returns>a new array containing the content to be cut</returns>
        public static T[] SubArray<T>(this T[] input, int index, int length)
        {
            var result = new T[length];
            Array.Copy(input, index, result, 0, length);
            return result;
        }

        #endregion Methods
    }
}
