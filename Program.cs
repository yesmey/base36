using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HelloThere
{
	class Base36
	{
		private static ReadOnlySpan<byte> Base36Char => new byte[36] { // uses C# compiler's optimization for static byte[] data
			(byte)'0', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5',
			(byte)'6', (byte)'7', (byte)'8', (byte)'9', (byte)'A', (byte)'B',
			(byte)'C', (byte)'D', (byte)'E', (byte)'F', (byte)'G', (byte)'H',
			(byte)'I', (byte)'J', (byte)'K', (byte)'L', (byte)'M', (byte)'N',
			(byte)'O', (byte)'P', (byte)'Q', (byte)'R', (byte)'S', (byte)'T',
			(byte)'U', (byte)'V', (byte)'W', (byte)'X', (byte)'Y', (byte)'Z'
		};

		public static string Create(long input)
		{
			var length = IterationLength(input);
			return string.Create(length, input, (stringSpan, inputValue) =>
			{
				ref char last = ref Unsafe.Add(ref MemoryMarshal.GetReference(stringSpan), stringSpan.Length - 1);
				while (inputValue != 0)
				{
					inputValue = Math.DivRem(inputValue, Base36Char.Length, out var index);
					last = (char)Base36Char[(int)index];
					last = ref Unsafe.Subtract(ref last, 1);
				}
			});
		}

		private static int IterationLength(long value)
		{
			const long p01 = 36L;
			const long p02 = 1296L;
			const long p03 = 46656L;
			const long p04 = 1679616L;
			const long p05 = 60466176L;
			const long p06 = 2176782336L;
			const long p07 = 78364164096L;
			const long p08 = 2821109907456L;
			const long p09 = 101559956668416L;
			const long p10 = 3656158440062976L;
			const long p11 = 131621703842267136L;
			const long p12 = 4738381338321616896L;

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
	}
}