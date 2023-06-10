using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Yesmey;

public static class Base36
{
    private const int Base36Length = 36;

    private static ReadOnlySpan<byte> Base36Char => "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"u8;

    private static ReadOnlySpan<ulong> Pow36 => new ulong[14]
    {
        0UL, // padding needed because decoding starts at 1
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

    [SkipLocalsInit]
    public static string Encode(ulong input)
    {
        Span<char> buffer = stackalloc char[16];

        int offset = buffer.Length - 1;
        while (input != 0)
        {
            ulong index;
            (input, index) = Math.DivRem(input, Base36Length);
            buffer[offset--] = (char)Base36Char[(int)index];
        }
        return new string(buffer.Slice(offset + 1));
    }

    public static ulong Decode(ReadOnlySpan<char> base36Value)
    {
        if (base36Value.IsEmpty)
        {
            return 0;
        }

        ulong sum = 0;
        for (var i = 1; i <= base36Value.Length; i++)
        {
            char value = base36Value[^i];
            if (!char.IsAsciiLetterOrDigit(value))
            {
                ThrowInvalidFormat();
            }

            const int lastDigit = '9' - '0';
            int pow = value - '0';
            if (pow > lastDigit)
            {
                const int spaceBetweenDigitsAndChars = 'A' - '9' - 1;
                pow -= spaceBetweenDigitsAndChars;
            }

            sum += (ulong)pow * Pow36[i];
        }

        return sum;
    }

    [DoesNotReturn]
    private static void ThrowInvalidFormat()
    {
        throw new FormatException("Unexpected character encountered");
    }
}
