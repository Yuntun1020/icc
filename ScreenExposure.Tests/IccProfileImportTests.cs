using System.Text;
using ScreenExposure.Core;

namespace ScreenExposure.Tests;

public sealed class IccProfileImportTests
{
    [Theory]
    [InlineData("filter.icc")]
    [InlineData("filter.icm")]
    [InlineData("FILTER.ICC")]
    public void ValidateImportCandidate_AcceptsIccAndIcmWithAcspSignature(string fileName)
    {
        var profile = CreateMinimalProfile();

        var result = IccProfileImport.ValidateImportCandidate(fileName, profile);

        Assert.Equal(fileName, result.FileName);
        Assert.Equal(Path.GetExtension(fileName).ToLowerInvariant(), result.Extension);
    }

    [Fact]
    public void ValidateImportCandidate_RejectsUnsupportedExtension()
    {
        var profile = CreateMinimalProfile();

        var exception = Assert.Throws<ArgumentException>(() =>
            IccProfileImport.ValidateImportCandidate("filter.txt", profile));

        Assert.Contains(".icc", exception.Message);
    }

    [Fact]
    public void ValidateImportCandidate_RejectsMissingAcspSignature()
    {
        var profile = CreateMinimalProfile();
        profile[36] = (byte)'x';

        var exception = Assert.Throws<ArgumentException>(() =>
            IccProfileImport.ValidateImportCandidate("filter.icc", profile));

        Assert.Contains("acsp", exception.Message);
    }

    [Fact]
    public void SanitizeFileName_ReplacesInvalidCharactersAndKeepsExtension()
    {
        var sanitized = IccProfileImport.SanitizeFileName("bad:name?.icc");

        Assert.Equal("bad_name_.icc", sanitized);
    }

    private static byte[] CreateMinimalProfile()
    {
        var profile = new byte[132];
        profile[0] = 0;
        profile[1] = 0;
        profile[2] = 0;
        profile[3] = 132;
        Encoding.ASCII.GetBytes("acsp", profile.AsSpan(36, 4));
        return profile;
    }
}
