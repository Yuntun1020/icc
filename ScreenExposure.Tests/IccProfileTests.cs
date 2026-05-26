using System.Text;
using ScreenExposure.Core;

namespace ScreenExposure.Tests;

public sealed class IccProfileTests
{
    [Fact]
    public void BuildVcgtTag_EncodesTableHeaderAndRgbData()
    {
        var ramp = GammaRamp.Generate(ColorAdjustment.Default with { ExposureStops = -1.0 });

        var tag = IccProfileBuilder.BuildVcgtTag(ramp);

        Assert.Equal("vcgt", Encoding.ASCII.GetString(tag.AsSpan(0, 4)));
        Assert.Equal(0u, ReadUInt32(tag, 8));
        Assert.Equal(3, ReadUInt16(tag, 12));
        Assert.Equal(256, ReadUInt16(tag, 14));
        Assert.Equal(2, ReadUInt16(tag, 16));
        Assert.Equal(ramp.Red[128], ReadUInt16(tag, 18 + (128 * 2)));
        Assert.Equal(ramp.Green[128], ReadUInt16(tag, 18 + (256 * 2) + (128 * 2)));
        Assert.Equal(ramp.Blue[128], ReadUInt16(tag, 18 + (512 * 2) + (128 * 2)));
    }

    [Fact]
    public void WithVcgtTag_AddsTagEntryAndUpdatesProfileSize()
    {
        var profile = CreateProfileWithEmptyTagTable();
        var ramp = GammaRamp.Generate(ColorAdjustment.Default);

        var updated = IccProfileBuilder.WithVcgtTag(profile, ramp);

        Assert.Equal(updated.Length, (int)ReadUInt32(updated, 0));
        Assert.Equal(1u, ReadUInt32(updated, 128));
        Assert.Equal("vcgt", Encoding.ASCII.GetString(updated.AsSpan(132, 4)));
        Assert.Equal("vcgt", Encoding.ASCII.GetString(updated.AsSpan((int)ReadUInt32(updated, 136), 4)));
    }

    private static byte[] CreateProfileWithEmptyTagTable()
    {
        var profile = new byte[132];
        WriteUInt32(profile, 0, (uint)profile.Length);
        WriteAscii(profile, 36, "acsp");
        WriteUInt32(profile, 128, 0);
        return profile;
    }

    private static ushort ReadUInt16(byte[] data, int offset)
    {
        return (ushort)((data[offset] << 8) | data[offset + 1]);
    }

    private static uint ReadUInt32(byte[] data, int offset)
    {
        return ((uint)data[offset] << 24)
            | ((uint)data[offset + 1] << 16)
            | ((uint)data[offset + 2] << 8)
            | data[offset + 3];
    }

    private static void WriteUInt32(byte[] data, int offset, uint value)
    {
        data[offset] = (byte)(value >> 24);
        data[offset + 1] = (byte)(value >> 16);
        data[offset + 2] = (byte)(value >> 8);
        data[offset + 3] = (byte)value;
    }

    private static void WriteAscii(byte[] data, int offset, string value)
    {
        Encoding.ASCII.GetBytes(value, data.AsSpan(offset, value.Length));
    }
}
