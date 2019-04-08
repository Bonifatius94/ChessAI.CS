using Chess.AI.Data;
using Chess.Lib.Extensions;
using System;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace Chess.UnitTest
{
    public class BitboardTest : TestBase
    {
        #region Init

        public BitboardTest(ITestOutputHelper output) : base(output) { }

        #endregion Init

        #region Stopwatch

        private static int singleBitsRead = 0;
        private static int singleBitsWritten = 0;
        private static int multipleBitsRead = 0;
        private static int multipleBitsWritten = 0;

        //private static readonly Stopwatch stopwatchReadSingleBit = new Stopwatch();
        //private static readonly Stopwatch stopwatchWriteSingleBit = new Stopwatch();
        //private static readonly Stopwatch stopwatchReadMultipleBits = new Stopwatch();
        //private static readonly Stopwatch stopwatchWriteSMultipleBits = new Stopwatch();

        #endregion Stopwatch

        #region Methods

        // init random number generator
        private static readonly Random random = new Random();

        [Fact]
        public void BitboardTests()
        {
            var stopwatch = new Stopwatch();
            
            // init the raw bitboard data
            byte[] data = new byte[100];
            
            // test bitboard with various data
            for (int i = 0; i < 10000; i++)
            {
                // get random binary data
                random.NextBytes(data);

                // init bitboard from random binary data
                int length = (data.Length * 8) - (i % 8);

                stopwatch.Start();
                var bitboard = new Bitboard(data, data.Length * 8 - i % 8);
                stopwatch.Stop();
                
                // test retrieving single bits
                for (int j = 0; j < length; j++)
                {
                    // determine whether the original bit is set
                    byte cache = data[j / 8];
                    bool originalBit = (cache & (byte)(1 << (7 - j % 8))) > 0;

                    stopwatch.Start();
                    // retrieve the bit from the bitboard
                    bool temp = bitboard.IsBitSetAt(j);
                    stopwatch.Stop();
                    singleBitsRead++;

                    // check if the bitboard returns the same value
                    Assert.True(temp == originalBit);
                }

                // test changing single bits
                for (int j = 0; j < length; j++)
                {
                    // choose a random bit to be applied
                    bool newBit = random.Next(0, 2) == 1;
                    
                    stopwatch.Start();
                    // apply the bit to the bitboard and retrieve it afterwards
                    bitboard.SetBitAt(j, newBit);
                    stopwatch.Stop();
                    singleBitsWritten++;

                    // check whether the bit was applied correctly
                    Assert.True(bitboard.IsBitSetAt(j) == newBit);
                }

                // test if access array index operator is working
                for (int j = 0; j < length; j++)
                {
                    // choose a random bit to be applied
                    bool newBit = random.Next(0, 2) == 1;

                    stopwatch.Start();
                    // apply the bit to the bitboard and retrieve it afterwards
                    bitboard[j] = newBit;
                    bool temp = bitboard[j];
                    stopwatch.Stop();
                    singleBitsRead++;
                    singleBitsWritten++;

                    // check whether the bit was applied correctly
                    Assert.True(temp == newBit);
                }

                // test reading multiple bits (up to a whole byte)
                for (int j = 0; j < length; j++)
                {
                    // test retrieving 1 to 8 bits
                    for (int n = 1; n < 9; n++)
                    {
                        try
                        {
                            stopwatch.Start();
                            // retrieve bits from bitboard
                            byte bits = bitboard.GetBitsAt(j, n);
                            stopwatch.Stop();
                            multipleBitsRead++;

                            // determine which bits should be set for comparison
                            byte cmp = 0;
                            for (int m = 0; m < n; m++) { cmp = (byte)((cmp << 1) | (bitboard[j + m] ? 1 : 0)); }
                            cmp = (byte)(cmp << (8 - n));

                            // check whether the correct bits were retrieved
                            Assert.True(bits == cmp);
                        }
                        catch (Exception ex)
                        {
                            // determine whether the exception was expected
                            bool isCorrectOutOfBoundsError = (j + n >= length);

                            // write some variables to trace for error debugging
                            if (!isCorrectOutOfBoundsError) { output.WriteLine($"retrieving bits failed! binary data: { bitboard.BinaryData.BytesToHexString() }, j = { j }, n = { n }, length = { length }"); }

                            // check whether an array out of bounds exception was correctly thrown
                            Assert.True(isCorrectOutOfBoundsError);
                        }
                    }
                }

                // test writing multiple bits (up to a whole byte)
                for (int j = 0; j < length; j++)
                {
                    // test retrieving 1 to 8 bits
                    for (int n = 1; n < 9; n++)
                    {
                        // choose random bits to write
                        byte bitsToWrite = (byte)((random.Next(0, (1 << n))) << (8 - n));

                        try
                        {
                            stopwatch.Start();
                            // apply the bits to the bitboard
                            bitboard.SetBitsAt(j, bitsToWrite, n);

                            // retrieve the bits from the bitboard
                            byte cmp = bitboard.GetBitsAt(j, n);
                            stopwatch.Stop();

                            multipleBitsRead++;
                            multipleBitsWritten++;

                            // check whether the bits were correctly set
                            Assert.True(bitsToWrite == cmp);
                        }
                        catch (Exception ex)
                        {
                            // determine whether the exception was expected
                            bool isCorrectOutOfBoundsError = (j + n >= length);

                            // write some variables to trace for error debugging
                            if (!isCorrectOutOfBoundsError) { output.WriteLine($"retrieving bits failed! binary data: { bitboard.BinaryData.BytesToHexString() }, j = { j }, n = { n }, length = { length }"); }

                            // check whether an array out of bounds exception was correctly thrown
                            Assert.True(isCorrectOutOfBoundsError);
                        }
                    }
                }
            }
            
            var time = new TimeSpan(stopwatch.ElapsedTicks);
            int totalOperations = singleBitsRead + singleBitsWritten + multipleBitsRead + multipleBitsWritten;
            double timePerOperationInMs = time.TotalMilliseconds / totalOperations;
            double operationsPerMin = 1000 / timePerOperationInMs;

            output.WriteLine($"total time elapsed for bitboard operations: { time.TotalMilliseconds } ms");
            output.WriteLine($"time per operation: { timePerOperationInMs } ms ({ operationsPerMin } operations / sec)");
        }

        [Fact]
        public void EdgeCaseTest()
        {
            // retrieving bits failed! binary data: 095A, j = 0, n = 7, length = 16 => expected result = 0x08
            executeEdgeCaseTest(new byte[] { 0x09, 0x5A }, 16, 0, 7, 0x08);

            // retrieving bits failed! binary data: 19FE4AE0D43A91B00054, j = 9, n = 8, length = 80 => expected result = 0xFC
            executeEdgeCaseTest(new byte[] { 0x19, 0xFE, 0x4A, 0xE0, 0xD4, 0x3A, 0x91, 0xB0, 0x00, 0x54 }, 80, 9, 8, 0xFC);
        }

        private void executeEdgeCaseTest(byte[] data, int bitboardLength, int index, int length, byte expectedResult)
        {
            // init bitboard
            var bitboard = new Bitboard(data, bitboardLength);

            // simulate the bitwise access
            byte cache = bitboard.GetBitsAt(index, length);

            // check if the correct result was returned
            Assert.True(cache == expectedResult);
        }

        #region Helpers

        //private bool readSingleBitFromBitboard(Bitboard bitboard, int index)
        //{
        //    // start stopwatch
        //    stopwatchReadSingleBit.Start();

        //    // get bit from bitboard
        //    bool bit = bitboard.IsBitSetAt(index);

        //    // stop stopwatch and increment the operations counter
        //    stopwatchReadSingleBit.Stop();
        //    singleBitsRead++;

        //    return bit;
        //}

        //private void writeSingleBitToBitboard(Bitboard bitboard, int index, bool bit)
        //{
        //    // start stopwatch
        //    stopwatchWriteSingleBit.Start();

        //    // apply the bit to the bitboard
        //    bitboard.SetBitAt(index, bit);

        //    // stop stopwatch and increment the operations counter
        //    stopwatchWriteSingleBit.Stop();
        //    singleBitsWritten++;
        //}

        //private byte readMultipleBitsFromBitboard(Bitboard bitboard, int index, int length)
        //{
        //    // start stopwatch
        //    stopwatchReadMultipleBits.Start();

        //    // get bits from bitboard
        //    byte bits = bitboard.GetBitsAt(index, length);

        //    // stop stopwatch and increment the operations counter
        //    stopwatchReadMultipleBits.Stop();
        //    multipleBitsRead++;

        //    return bits;
        //}

        //private void writeMultipleBitsToBitboard(Bitboard bitboard, int index, byte bits, int length)
        //{
        //    // start stopwatch
        //    stopwatchWriteSMultipleBits.Start();

        //    // apply the bit to the bitboard
        //    bitboard.SetBitsAt(index, bits, length);

        //    // stop stopwatch and increment the operations counter
        //    stopwatchWriteSMultipleBits.Stop();
        //    multipleBitsWritten++;
        //}

        //private double getAverageOperationTimeInMs(Stopwatch stopwatch, int operationsCount)
        //{
        //    var time = new TimeSpan(stopwatch.ElapsedTicks);
        //    return time.TotalMilliseconds / operationsCount;
        //}

        //private double getOperationsPerSecond(Stopwatch stopwatch, int operationsCount)
        //{
        //    return 1000 / getAverageOperationTimeInMs(stopwatch, operationsCount);
        //}

        #endregion Helpers

        #endregion Methods
    }
}
