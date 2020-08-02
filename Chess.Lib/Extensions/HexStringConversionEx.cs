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
using System.Text;

namespace Chess.Lib.Extensions
{
    /// <summary>
    /// Provide conversion functionality between hex strings and binary byte arrays.
    /// </summary>
    public static class HexStringConversionEx
    {
        #region Methods

        /// <summary>
        /// Convert a hex string to a binary byte array.
        /// </summary>
        /// <param name="hexString">The hex string containing the data.</param>
        /// <returns>a binary byte array</returns>
        public static byte[] HexStringToBytes(this string hexString)
        {
            // init binary data array
            int bytesCount = (hexString.Length / 2) + (hexString.Length % 2 > 0 ? 1 : 0);
            var data = new byte[bytesCount];

            // loop through all hex digits
            for (int i = 0; i < hexString.Length; i += 2)
            {
                // get upper and lower nibble
                byte upper = (byte)(getHexByte(hexString[i]) << 4);
                byte lower = (i + 1 < hexString.Length) ? getHexByte(hexString[i + 1]) : (byte)0;

                // append the two corresponding hex characters
                data[i / 2] = (byte)(upper | lower);
            }

            return data;
        }

        /// <summary>
        /// Convert a binary byte array to a hex string.
        /// </summary>
        /// <param name="data">The binary byte array containing the data.</param>
        /// <returns>a hex string</returns>
        public static string BytesToHexString(this byte[] data)
        {
            var builder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                // get upper and lower nibble
                byte upper = (byte)((data[i] & 0xF0) >> 4);
                byte lower = (byte)(data[i] & 0x0F);

                // append the two corresponding hex characters
                builder.Append($"{ getHexChar(upper) }{ getHexChar(lower) }");
            }

            return builder.ToString();
        }

        #region Helpers

        private static byte getHexByte(char hex)
        {
            return (byte)(hex - ((hex >= '0' && hex <= '9') ? '0' : ((hex >= 'A' && hex <= 'F') ? 'A' : 'a')));
        }

        private static char getHexChar(byte data)
        {
            return (char)((data < 10) ? ('0' + data) : ('A' + data - 10));
        }

        #endregion Helpers

        #endregion Methods
    }
}
