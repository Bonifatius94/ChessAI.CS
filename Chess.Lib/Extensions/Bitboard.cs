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

using Chess.Lib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Chess.Lib.Extensions
{
    // TODO: replace this by BigInteger struct

    /// <summary>
    /// Provide bitwise operations for a concatenation of data bits.
    /// </summary>
    public struct Bitboard : IEnumerable<bool>, ICloneable
    {
        #region Constructor

        /// <summary>
        /// Create a new, empty bitboard of the given length.
        /// </summary>
        /// <param name="bitsCount">The length of the bitboard.</param>
        public Bitboard(int bitsCount) : this(new byte[(bitsCount / 8) + (bitsCount % 8 > 0 ? 1 : 0)], bitsCount) { }

        /// <summary>
        /// Create a new bitboard with the given bianry data.
        /// </summary>
        /// <param name="binaryData">The binary data containing the bits of the bitboard.</param>
        /// <param name="bitsCount">The exact length of the bitboard. (default: binary data array length * 8)</param>
        public Bitboard(byte[] binaryData, int bitsCount = 0)
        {
            // copy the binary data
            BinaryData = new byte[binaryData.Length];
            Array.Copy(binaryData, BinaryData, binaryData.Length);

            // determine the bitboard length
            Length = (bitsCount > 0) ? bitsCount : binaryData.Length * 8;
        }
        
        #endregion Constructor

        #region Members

        /// <summary>
        /// The bits storage of the bitboard.
        /// </summary>
        public byte[] BinaryData { get; private set; }

        /// <summary>
        /// The amount of bits stored in the bitboard.
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// Get or set a single bit by index like a boolean array.
        /// </summary>
        /// <param name="index">The index of the bit to be get or set.</param>
        /// <returns>a single bit at the given index</returns>
        public bool this[int index]
        {
            get { return IsBitSetAt(index); }
            set { SetBitAt(index, value); }
        }

        #endregion Members

        #region Methods

        #region Read

        /// <summary>
        /// Determine whether the bit at the given index is set.
        /// </summary>
        /// <param name="index">The index of the bit.</param>
        /// <returns>a boolean indicating whether the bit is set.</returns>
        public bool IsBitSetAt(int index)
        {
            // make sure the index is within range
            if (index < 0 || index >= Length) { throw new ArgumentException("index out of bitboard range"); }

            // determine the byte and the bit index
            int byteIndex = index / 8;
            int bitIndexOfByte = index % 8;

            // determine whether the bit at the given index is set or not set
            byte and = (byte)(1 << (7 - bitIndexOfByte));
            bool bit = ((BinaryData[byteIndex] & and) > 0);

            return bit;
        }

        /// <summary>
        /// Get multiple bits up to a whole byte, starting at the given index.
        /// </summary>
        /// <param name="index">The index to start reading.</param>
        /// <param name="length">The amount of bits to read.</param>
        /// <returns>a byte containing the bits (starting with higher value bits)</returns>
        public byte GetBitsAt(int index, int length = 8)
        {
            // make sure the index is within range
            if (index < 0 || index + length > Length) { throw new ArgumentException("index out of bitboard range"); }

            // load data bytes into cache
            byte upper = BinaryData[index / 8];
            byte lower = (index / 8 + 1 < BinaryData.Length) ? BinaryData[index / 8 + 1] : (byte)0x00;
            int bitOffset = index % 8;

            // cut the bits from the upper byte
            byte upperDataMask = (byte)((1 << (8 - bitOffset)) - 1);
            int lastIndexOfByte = bitOffset + length - 1;
            if (lastIndexOfByte < 7) { upperDataMask = (byte)((upperDataMask >> (7 - lastIndexOfByte)) << (7 - lastIndexOfByte)); }
            byte upperData = (byte)((upper & upperDataMask) << (bitOffset));

            // cut bits from the lower byte (if needed, otherwise set all bits 0)
            byte lowerDataMask = (byte)(0xFF << (16 - bitOffset - length));
            byte lowerData = (byte)((lower & lowerDataMask) >> (8 - bitOffset));

            // put the data bytes together (with bitwise OR)
            byte data = (byte)(upperData | lowerData);
            return data;
        }
        
        #endregion Read

        #region Write

        /// <summary>
        /// Set a single bit of the bitboard at the given index.
        /// </summary>
        /// <param name="index">The index of the bit.</param>
        /// <param name="bit">Indicates whether the bit should be set.</param>
        public void SetBitAt(int index, bool bit)
        {
            // make sure the index is within range
            if (index < 0 || index >= Length) { throw new ArgumentException("index out of bitboard range"); }

            // determine the byte and the bit index
            int byteIndex = index / 8;
            int bitIndexOfByte = index % 8;

            // set the bit
            byte oldData = BinaryData[byteIndex];
            byte and = (byte)(1 << (7 - bitIndexOfByte));
            byte newData = (byte)((oldData & ~and) | ((bit ? 0xFF : 0x00) & and));
            BinaryData[byteIndex] = newData;
        }

        /// <summary>
        /// Set multiple bits (according to length) up to a whole byte to the bitboard at the given start index.
        /// </summary>
        /// <param name="index">The start index of the data to write.</param>
        /// <param name="newData">The data to write (data is starting at the highest value bit of the byte).</param>
        /// <param name="length">The length of the data to write.</param>
        public void SetBitsAt(int index, byte newData, int length = 8)
        {
            // make sure the index is within range
            if (index < 0 || index + length > Length) { throw new ArgumentException("index out of bitboard range"); }

            // compute initial data byte index containing the first bits to be written
            int byteIndex = index / 8;
            int i = 0;

            // loop through all data bytes to be written (max 2 bytes)
            do
            {
                // init data cache with the old data byte
                byte dataCache = BinaryData[byteIndex];

                // loop through all bits of the binary data byte (quit loop when end of data byte is reached)
                for (; i < length && index < (byteIndex + 1) * 8; i++, index++)
                {
                    // determine whether the bit should be set or not
                    byte bitOfInputByte = (byte)(1 << (7 - i));
                    bool isBitSet = (newData & bitOfInputByte) > 0;

                    // set the bit accordingly
                    byte bitOfDataByte = (byte)(1 << (7 - index % 8));
                    dataCache = (byte)((dataCache & ~bitOfDataByte) | ((isBitSet ? 0xFF : 0x00) & bitOfDataByte));
                }

                // apply the bits to the binary data array
                BinaryData[byteIndex++] = dataCache;
            }
            while (i < length);
        }

        #endregion Write

        #region Enumerable

        /// <summary>
        /// Retrieve a new bitboard enumerator instance with the data of this instance.
        /// </summary>
        /// <returns>a new bitboard enumerator instance</returns>
        public IEnumerator<bool> GetEnumerator()
        {
            return new BitboardEnumerator(this);
        }

        /// <summary>
        /// Retrieve a new bitboard enumerator instance with the data of this instance.
        /// </summary>
        /// <returns>a new bitboard enumerator instance</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class BitboardEnumerator : IEnumerator<bool>
        {
            #region Constructor

            // init enumerator with bitboard data
            public BitboardEnumerator(Bitboard data) { _data = data; }

            #endregion Constructor

            #region Members

            private const int START_INDEX = -1;
            private int _index = START_INDEX;
            private Bitboard _data;
            private byte _cache = 0;
            
            public bool Current
            {
                get
                {
                    // retrieve the bit at the current index (from cached data byte)
                    byte and = (byte)(1 << (7 - _index % 8));
                    bool bit = ((_cache & and) > 0);
                    return bit;
                }
            }

            object IEnumerator.Current { get { return Current; } }

            #endregion Members

            public void Dispose()
            {
                // nothing to do here ...
                // everything is stored onto stack
            }

            public bool MoveNext()
            {
                // increment index and check whether it is still within bitboard range
                bool canMoveNext = ++_index < _data.Length;

                // load next byte into cache
                if (_index % 8 == 0) { _cache = _data.BinaryData[_index / 8]; }
                
                return canMoveNext;
            }

            public void Reset()
            {
                // reset the index to the start value
                _index = START_INDEX;
            }
        }

        #endregion Enumerable

        #region Extensions

        /// <summary>
        /// Create a new bitboard instance of the given length containing all bits of the current instance starting at the given index.
        /// </summary>
        /// <param name="index">The start index of the bits to be cut.</param>
        /// <param name="length">The length of the bits to be cut.</param>
        /// <returns>a sub-instance of the current board</returns>
        public Bitboard SubBoard(int index, int length)
        {
            // make sure the index is within range
            if (index < 0 || index + length >= Length) { throw new ArgumentException("index out of bitboard range"); }
            
            // init data array
            var data = new byte[length / 8 + ((length % 8 > 0) ? 1 : 0)];
            int i;

            // copy whole data bytes until last byte
            for (i = 0; i < length - 8; i += 8)
            {
                // determine whether the bit is set in the original board
                byte dataByte = GetBitsAt(index + i);
                data[i / 8] = dataByte;
            }

            // create a new bitboard with the given length
            var board = new Bitboard(data, length);

            // copy the last bits
            for (; i < length; i++)
            {
                // determine whether the bit is set in the original board and apply it to the new board
                bool bit = IsBitSetAt(index + i);
                board.SetBitAt(i, bit);
            }

            return board;
        }

        /// <summary>
        /// Concatenate another bitboard and return the result as a new bitboard instance.
        /// </summary>
        /// <param name="concat">The first bitboard.</param>
        /// <returns>a combined bitboard</returns>
        public Bitboard Concat(Bitboard concat)
        {
            // copy the data of this board
            int bitsCount = Length + concat.Length;
            var bytes = new byte[(bitsCount / 8) + (bitsCount % 8 > 0 ? 1 : 0)];
            Array.Copy(BinaryData, bytes, Length);

            // create a new bitboard with sufficient length
            var board = new Bitboard(bytes, bitsCount);

            // apply the data of the board to be concatenated
            for (int i = 0; i < concat.Length; i++)
            {
                bool bit = concat.IsBitSetAt(i);
                board.SetBitAt(Length + i, bit);
            }

            return board;
        }

        /// <summary>
        /// Create a deep copy of the current bitboard instance.
        /// </summary>
        /// <returns>a deep copy of the bitboard</returns>
        public object Clone()
        {
            return new Bitboard(BinaryData, Length);
        }

        #endregion Extensions

        #endregion Methods
    }
}
