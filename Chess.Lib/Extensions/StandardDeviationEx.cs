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
using System.Text;

namespace Chess.Lib.Extensions
{
    /// <summary>
    /// An extension providing standard deviation computation functionality.
    /// </summary>
    public static class StandardDeviationEx
    {
        #region Methods

        /// <summary>
        /// Compute the expectation of the given (value, probability) tuples.
        /// </summary>
        /// <param name="values">The list of (value, probability) tuples to be evaluated</param>
        /// <returns>The expectation of the given tuples</returns>
        public static double Expectation(this IEnumerable<Tuple<double, double>> values)
        {
            // list of values as (value, probability) tuples
            return values.Select(x => x.Item1 * x.Item2).Sum();
        }

        /// <summary>
        /// Compute the standard deviation of the given (value, probability) tuples.
        /// </summary>
        /// <param name="values">The list of (value, probability) tuples to be evaluated</param>
        /// <returns>The standard deviation of the given tuples</returns>
        public static double StandardDeviation(this IEnumerable<Tuple<double, double>> values)
        {
            double exp = Expectation(values);
            double variance = values.Select(x => Math.Pow((x.Item1 - exp), 2) * x.Item2).Sum();
            return Math.Sqrt(variance);
        }

        #endregion Methods
    }
}
