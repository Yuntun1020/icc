using System.Text;

namespace ScreenExposure.Core;

public static class IccProfileBuilder
{
    private const int HeaderSize = 128;
    private const string VcgtSignature = "vcgt";

    public static byte[] BuildVcgtTag(GammaRamp ramp)
    {
        ArgumentNullException.ThrowIfNull(ramp);
        ValidateRamp(ramp);

        var bytes = new byte[18 + (ramp.Red.Length * 3 * sizeof(ushort))];
        WriteAscii(bytes, 0, VcgtSignature);
        WriteUInt32(bytes, 8, 0);
        WriteUInt16(bytes, 12, 3);
        WriteUInt16(bytes, 14, (ushort)ramp.Red.Length);
        WriteUInt16(bytes, 16, 2);

        var offset = 18;
        WriteCurve(bytes, ref offset, ramp.Red);
        WriteCurve(bytes, ref offset, ramp.Green);
        WriteCurve(bytes, ref offset, ramp.Blue);
        return bytes;
    }

    public static byte[] WithVcgtTag(byte[] sourceProfile, GammaRamp ramp)
    {
        ArgumentNullException.ThrowIfNull(sourceProfile);
        ArgumentNullException.ThrowIfNull(ramp);
        if (sourceProfile.Length < HeaderSize + 4)
        {
            throw new ArgumentException("ICC profile is too small to contain a header and tag table.", nameof(sourceProfile));
        }

        if (ReadAscii(sourceProfile, 36, 4) != "acsp")
        {
            throw new ArgumentException("ICC profile does not contain the acsp signature.", nameof(sourceProfile));
        }

        var sourceTagCount = ReadUInt32(sourceProfile, HeaderSize);
        var sourceTagTableSize = checked(4 + ((int)sourceTagCount * 12));
        if (sourceProfile.Length < HeaderSize + sourceTagTableSize)
        {
            throw new ArgumentException("ICC profile tag table is truncated.", nameof(sourceProfile));
        }

        var entries = ReadTagEntries(sourceProfile, (int)sourceTagCount)
            .Where(entry => entry.Signature != VcgtSignature)
            .ToList();

        var vcgtTag = BuildVcgtTag(ramp);
        var tagCount = entries.Count + 1;
        var tagTableSize = 4 + (tagCount * 12);
        var dataStart = Align4(HeaderSize + tagTableSize);
        var totalLength = dataStart + entries.Sum(entry => Align4((int)entry.Size)) + Align4(vcgtTag.Length);
        var result = new byte[totalLength];

        Array.Copy(sourceProfile, result, Math.Min(sourceProfile.Length, HeaderSize));
        WriteUInt32(result, HeaderSize, (uint)tagCount);

        var dataOffset = dataStart;
        var tableOffset = HeaderSize + 4;
        foreach (var entry in entries)
        {
            var sourceOffset = (int)entry.Offset;
            var size = (int)entry.Size;
            if (sourceOffset < 0 || size < 0 || sourceOffset + size > sourceProfile.Length)
            {
                throw new ArgumentException($"ICC tag {entry.Signature} points outside the profile data.", nameof(sourceProfile));
            }

            WriteTagEntry(result, tableOffset, entry.Signature, dataOffset, size);
            Array.Copy(sourceProfile, sourceOffset, result, dataOffset, size);
            tableOffset += 12;
            dataOffset += Align4(size);
        }

        WriteTagEntry(result, tableOffset, VcgtSignature, dataOffset, vcgtTag.Length);
        Array.Copy(vcgtTag, 0, result, dataOffset, vcgtTag.Length);
        WriteUInt32(result, 0, (uint)result.Length);
        return result;
    }

    private static List<TagEntry> ReadTagEntries(byte[] profile, int tagCount)
    {
        var entries = new List<TagEntry>(tagCount);
        for (var index = 0; index < tagCount; index++)
        {
            var offset = HeaderSize + 4 + (index * 12);
            entries.Add(new TagEntry(
                ReadAscii(profile, offset, 4),
                ReadUInt32(profile, offset + 4),
                ReadUInt32(profile, offset + 8)));
        }

        return entries;
    }

    private static void ValidateRamp(GammaRamp ramp)
    {
        if (ramp.Red.Length != ramp.Green.Length || ramp.Red.Length != ramp.Blue.Length)
        {
            throw new ArgumentException("RGB gamma ramp channels must have the same length.", nameof(ramp));
        }

        if (ramp.Red.Length is < 2 or > ushort.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(ramp), "ICC vcgt table length must fit in a UInt16.");
        }
    }

    private static int Align4(int value)
    {
        return (value + 3) & ~3;
    }

    private static void WriteCurve(byte[] target, ref int offset, ushort[] curve)
    {
        foreach (var value in curve)
        {
            WriteUInt16(target, offset, value);
            offset += sizeof(ushort);
        }
    }

    private static void WriteTagEntry(byte[] target, int offset, string signature, int dataOffset, int size)
    {
        WriteAscii(target, offset, signature);
        WriteUInt32(target, offset + 4, (uint)dataOffset);
        WriteUInt32(target, offset + 8, (uint)size);
    }

    private static string ReadAscii(byte[] data, int offset, int count)
    {
        return Encoding.ASCII.GetString(data, offset, count);
    }

    private static void WriteAscii(byte[] data, int offset, string value)
    {
        Encoding.ASCII.GetBytes(value, data.AsSpan(offset, value.Length));
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

    private static void WriteUInt16(byte[] data, int offset, ushort value)
    {
        data[offset] = (byte)(value >> 8);
        data[offset + 1] = (byte)value;
    }

    private static void WriteUInt32(byte[] data, int offset, uint value)
    {
        data[offset] = (byte)(value >> 24);
        data[offset + 1] = (byte)(value >> 16);
        data[offset + 2] = (byte)(value >> 8);
        data[offset + 3] = (byte)value;
    }

    private readonly record struct TagEntry(string Signature, uint Offset, uint Size);
}
