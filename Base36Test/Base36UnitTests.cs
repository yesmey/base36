using Yesmey;

namespace Base36Test;

public class Base36UnitTests
{
    [Fact]
    public void EncodeLong()
    {
        Assert.Equal("ZIK0ZJ", Base36.Encode(2147483647));
        Assert.Equal("1Y2P0IJ32E8E7", Base36.Encode(9223372036854775807UL));
    }

    [Fact]
    public void EncodeEmpty()
    {
        Assert.Equal(string.Empty, Base36.Encode(0));
    }

    [Fact]
    public void DecodeLong()
    {
        Assert.Equal<ulong>(2147483647, Base36.Decode("ZIK0ZJ"));
        Assert.Equal(9223372036854775807UL, Base36.Decode("1Y2P0IJ32E8E7"));
    }

    [Fact]
    public void DecodeEmpty()
    {
        Assert.Equal(0ul, Base36.Decode(string.Empty));
    }
}
