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

        private static ReadOnlySpan<ulong> Pow36 => new ulong[13]
        {
            1UL,
            36UL,
            1296UL,
            46656UL,
            1679616UL,
            60466176UL,
            2176782336UL,
            78364164096UL,
            2821109907456UL,
            101559956668416UL,
            3656158440062976UL,
            131621703842267136UL,
            4738381338321616896UL
        };

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static string Encode(ulong input)
        {
            var length = IterationLength(input);
            return string.Create(length, input, (stringSpan, inputValue) =>
            {
                ref char lastChar = ref LastCharInSpan(stringSpan);
                while (inputValue != 0)
                {
                    inputValue = DivRem(inputValue, out var index);
                    lastChar = (char)Base36Char[(int)index];
                    lastChar = ref Unsafe.Subtract(ref lastChar, 1);
                }
            });
        }

        public static ulong Decode(ReadOnlySpan<char> base36Value)
        {
            if (base36Value.IsEmpty)
            {
                return ulong.MaxValue;
            }

            ref char lastChar = ref LastCharInSpan(base36Value);
            ref ulong pow = ref MemoryMarshal.GetReference(Pow36);

            ulong sum = 0;
            for (var i = 0; i < base36Value.Length; i++)
            {
                if (lastChar < '0' || lastChar > 'Z')
                {
                    return ulong.MaxValue;
                }

                // assume ascii
                var index = (byte)lastChar - '0';
                if (index > 10)
                {
                    index -= 7;
                }

                sum += (ulong)index * pow;
                pow = ref Unsafe.Add(ref pow, 1);
                lastChar = ref Unsafe.Subtract(ref lastChar, 1);
            }

            return sum;
        }

        private static int IterationLength(ulong value)
        {
            // constants of ReverseIterationLength
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

        // Math.DivRem ulong doesnt exist in netcore31?
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong DivRem(ulong a, out ulong result)
        {
            ulong div = a / Base36Length;
            result = a - (div * Base36Length);
            return div;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref char LastCharInSpan(ReadOnlySpan<char> span)
            => ref Unsafe.Add(ref MemoryMarshal.GetReference(span), span.Length - 1);
    }
}
