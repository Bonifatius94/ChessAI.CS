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
