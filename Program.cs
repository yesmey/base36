using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HelloThere
{
    public static class Base36
    {
        private const int Base36Length = 36;

        private static ReadOnlySpan<byte> Base36Char => new byte[Base36Length]
        {
            // abuse compiler optimization for static byte[] data
            (byte)'0', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5',
            (byte)'6', (byte)'7', (byte)'8', (byte)'9', (byte)'A', (byte)'B',
            (byte)'C', (byte)'D', (byte)'E', (byte)'F', (byte)'G', (byte)'H',
            (byte)'I', (byte)'J', (byte)'K', (byte)'L', (byte)'M', (byte)'N',
            (byte)'O', (byte)'P', (byte)'Q', (byte)'R', (byte)'S', (byte)'T',
            (byte)'U', (byte)'V', (byte)'W', (byte)'X', (byte)'Y', (byte)'Z'
        };

        [MethodImpl(MethodImplOptions.AggressiveOptimization)] // C# does some stupid stuff with the stack otherwise
        public static string Encode(ulong input)
        {
            var length = IterationLength(input);
            return string.Create(length, input, (stringSpan, inputValue) =>
            {
                ref char last = ref Unsafe.Add(ref MemoryMarshal.GetReference(stringSpan), stringSpan.Length - 1);
                while (inputValue != 0)
                {
                    inputValue = DivRem(inputValue, out var index);
                    last = (char)Base36Char[(int)index];
                    last = ref Unsafe.Subtract(ref last, 1);
                }
            });
        }

        public static ulong Decode(ReadOnlySpan<char> base36Value)
        {
            ref char last = ref Unsafe.Add(ref MemoryMarshal.GetReference(base36Value), base36Value.Length - 1);
            ulong sum = 0;
            for (var multiplier = 0; multiplier < base36Value.Length; multiplier++)
            {
                var index = (ulong)Base36Char.IndexOf((byte)last);
                sum += FastPow(index, multiplier);
                last = ref Unsafe.Subtract(ref last, 1);
            }

            return sum;
        }

        private static int IterationLength(ulong value)
        {
            const ulong p01 = 36UL;
            const ulong p02 = 1296UL;
            const ulong p03 = 46656UL;
            const ulong p04 = 1679616UL;
            const ulong p05 = 60466176UL;
            const ulong p06 = 2176782336UL;
            const ulong p07 = 78364164096UL;
            const ulong p08 = 2821109907456UL;
            const ulong p09 = 101559956668416UL;
            const ulong p10 = 3656158440062976UL;
            const ulong p11 = 131621703842267136UL;
            const ulong p12 = 4738381338321616896UL;

            if (value < p01) return 1;
            if (value < p02) return 2;
            if (value < p03) return 3;
            if (value < p04) return 4;
            if (value < p05) return 5;
            if (value < p06) return 6;
            if (value < p07) return 7;
            if (value < p08) return 8;
            if (value < p09) return 9;
            if (value < p10) return 10;
            if (value < p11) return 11;
            if (value < p12) return 12;
            return 13;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong FastPow(ulong index, int multiplier)
        {
            const double log36 = 3.58351893845611; // Math.Log(36)
            return index * (ulong)Math.Round(Math.Exp(multiplier * log36));
        }

        // Math.DivRem ulong doesnt exist in netcore31
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong DivRem(ulong a, out ulong result)
        {
            ulong div = a / Base36Length;
            result = a - (div * Base36Length);
            return div;
        }
    }
}
