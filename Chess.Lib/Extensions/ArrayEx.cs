using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Lib.Extensions
{
    /// <summary>
    /// An class with extension methods for arrays.
    /// </summary>
    public static class ArrayEx
    {
        #region Methods

        /// <summary>
        /// Copy a part of an array to a new array (similar to substring method).
        /// </summary>
        /// <typeparam name="T">The data type of the array. (if the type is a class, the resulting array is not a deep copy, just the object pointers get copied)</typeparam>
        /// <param name="array">The array to be copied.</param>
        /// <param name="startIndex">The starting index of the part to be copied (including).</param>
        /// <param name="length">The length of the part to be copied.</param>
        /// <returns>a part of the existing array as a copy</returns>
        public static T[] SubArray<T>(this T[] array, int startIndex, int length)
        {
            T[] result = new T[length];
            Array.Copy(array, startIndex, result, 0, length);
            return result;
        }

        #endregion Methods
    }
}
