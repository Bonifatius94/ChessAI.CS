using Chess.Lib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Chess.AI.Data
{
    /// <summary>
    /// Provide bitwise operations for a concatenation of data bits.
    /// </summary>
    public struct Bitboard : IEnumerable<bool>
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
            BinaryData = binaryData;
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
        /// <returns></returns>
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
            if (index < 0 || index + length >= Length) { throw new ArgumentException("index out of bitboard range"); }

            byte data = 0;

            // loop through all bits to be read
            for (int i = 0; i < length; i++)
            {
                // check if the bit is set
                if (IsBitSetAt(index + i))
                {
                    // apply the bit to the data byte
                    byte bitData = (byte)(1 << (7 - index));
                    data = (byte)(data | bitData);
                }
            }

            return data;
        }
        
        #endregion Read

        #region Write

        /// <summary>
        /// Set a single bit of the bitboard at the given index.
        /// </summary>
        /// <param name="index">The index of the bit.</param>
        /// <param name="bit">Indicates whether the bit should be set or not.</param>
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
            if (index < 0 || index + length >= Length) { throw new ArgumentException("index out of bitboard range"); }

            // loop through all bits to be set
            for (int i = 0; i < length; i++)
            {
                // determine whether the bit should be set or not
                byte and = (byte)(1 << (7 - i));
                bool bit = (newData & and) > 0;

                // set the bit accordingly
                SetBitAt(index + i, bit);
            }
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

            // retrieve the bit at the current index
            public bool Current { get { return _data.IsBitSetAt(_index); } }
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
                return ++_index < _data.Length;
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

            // create a new bitboard with the given length
            var board = new Bitboard(length);

            // loop through all bits
            for (int i = 0; i < length; i++)
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
        
        #endregion Extensions

        #endregion Methods
    }
}
