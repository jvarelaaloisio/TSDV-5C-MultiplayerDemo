using System;

namespace Network
{
    public static class ChecksumOperationsProvider
    {
        private delegate void Operation(ref ulong total, byte currentByte);

        private static Operation[] _operations1 = { Add, Subtract, BitShiftLeftBy3, BitShiftRightBy3, };
        private static Operation[] _operations2 = { BitShiftRightBy3, Add, BitShiftLeftBy3, Subtract, };
        private static void Add(ref ulong total, byte currentByte) => total += currentByte;
        private static void Subtract(ref ulong total, byte currentByte) => total -= currentByte;
        private static void BitShiftRightBy3(ref ulong total, byte currentByte) => total >>= 3;
        private static void BitShiftLeftBy3(ref ulong total, byte currentByte) => total <<= 3;

        public static bool IsValid(Span<Byte> bytes)
        {
            //TODO: Comparison between operations and lastBytes.
            //Halp
            return false;
        }
    }
}