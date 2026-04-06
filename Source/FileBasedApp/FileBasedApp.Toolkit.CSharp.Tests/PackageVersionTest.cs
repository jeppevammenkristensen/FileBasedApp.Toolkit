using System;
using FileBasedApp.Toolkit.CSharp;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit;

namespace FileBasedApp.Toolkit.CSharp.Tests;

[TestSubject(typeof(PackageVersion))]
public class PackageVersionTest
{
    #region Type detection

    [Theory]
    [InlineData("1.0.0")]
    [InlineData("12.3.4")]
    [InlineData("0.0.1")]
    [InlineData("1.0.0-beta")]
    [InlineData("1.0.0-alpha.1")]
    [InlineData("1.2.3.4")]
    [InlineData("1.2.3.4-rc.1")]
    [InlineData("1.0")]
    [InlineData("1")]
    public void Type_SemVerVersion_ReturnsSemVer(string version)
    {
        var sut = new PackageVersion(version);
        sut.Type.Should().Be(PackageVersionType.SemVer);
    }

    [Fact]
    public void Type_Star_ReturnsStar()
    {
        var sut = new PackageVersion("*");
        sut.Type.Should().Be(PackageVersionType.Star);
    }

    [Theory]
    [InlineData("abc")]
    
    [InlineData("")]
    [InlineData(null)]
    public void Type_InvalidVersion_ReturnsUnknown(string? version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            var act = () => new PackageVersion(version!);
            act.Should().Throw<ArgumentException>();
            return;
        }

        var sut = new PackageVersion(version);
        sut.Type.Should().Be(PackageVersionType.Unknown);
    }

    #endregion

    #region IsCatchAll

    [Fact]
    public void IsCatchAll_Star_ReturnsTrue()
    {
        var sut = new PackageVersion("*");
        sut.IsCatchAll.Should().BeTrue();
    }

    [Fact]
    public void IsCatchAll_SemVer_ReturnsFalse()
    {
        var sut = new PackageVersion("1.0.0");
        sut.IsCatchAll.Should().BeFalse();
    }

    #endregion

    #region Major / Minor / Patch / Revision

    [Fact]
    public void MajorMinorPatch_ThreePartVersion_ParsedCorrectly()
    {
        var sut = new PackageVersion("3.14.159");

        sut.Major.Should().Be("3");
        sut.Minor.Should().Be("14");
        sut.Patch.Should().Be("159");
        sut.Revision.Should().Be("0");
    }

    [Fact]
    public void Revision_FourPartVersion_ParsedCorrectly()
    {
        var sut = new PackageVersion("1.2.3.4");

        sut.Major.Should().Be("1");
        sut.Minor.Should().Be("2");
        sut.Patch.Should().Be("3");
        sut.Revision.Should().Be("4");
    }

    [Fact]
    public void MajorMinorPatch_StarVersion_AllNull()
    {
        var sut = new PackageVersion("*");

        sut.Major.Should().BeNull();
        sut.Minor.Should().BeNull();
        sut.Patch.Should().BeNull();
        sut.Revision.Should().BeNull();
    }

    #endregion

    #region VersionPrefix / VersionSuffix

    [Fact]
    public void VersionPrefix_ThreePartVersion_ReturnsFullVersion()
    {
        var sut = new PackageVersion("1.2.3");
        sut.VersionPrefix.Should().Be("1.2.3");
        sut.VersionSuffix.Should().BeNull();
    }

    [Fact]
    public void VersionPrefix_FourPartVersion_ReturnsFullVersion()
    {
        var sut = new PackageVersion("1.2.3.4");
        sut.VersionPrefix.Should().Be("1.2.3.4");
        sut.VersionSuffix.Should().BeNull();
    }

    [Fact]
    public void VersionPrefix_WithPrerelease_ReturnsOnlyNumericPart()
    {
        var sut = new PackageVersion("1.2.3-beta");
        sut.VersionPrefix.Should().Be("1.2.3");
    }

    [Fact]
    public void VersionSuffix_NoPrerelease_ReturnsEmpty()
    {
        var sut = new PackageVersion("1.2.3");
        sut.VersionSuffix.Should().BeNull();
    }

    [Theory]
    [InlineData("1.0.0-beta", "beta")]
    [InlineData("1.0.0-alpha.1", "alpha.1")]
    [InlineData("1.0.0-rc-2", "rc-2")]
    [InlineData("0.18.0-alpha-01", "alpha-01")]
    [InlineData("1.2.3.4-preview.3", "preview.3")]
    public void VersionSuffix_WithPrerelease_ReturnsSuffix(string version, string expectedSuffix)
    {
        var sut = new PackageVersion(version);
        sut.VersionSuffix.Should().Be(expectedSuffix);
    }

    [Fact]
    public void VersionSuffix_StarVersion_ReturnsNull()
    {
        var sut = new PackageVersion("*");
        sut.VersionSuffix.Should().BeNull();
    }

    #endregion

    #region Value reassignment

    [Fact]
    public void Value_Reassigned_UpdatesParsedProperties()
    {
        var sut = new PackageVersion("1.0.0");

        sut.Major.Should().Be("1");
        sut.Type.Should().Be(PackageVersionType.SemVer);

        sut.Value = "2.5.0-beta";

        sut.Major.Should().Be("2");
        sut.Minor.Should().Be("5");
        sut.Patch.Should().Be("0");
        sut.VersionSuffix.Should().Be("beta");
        sut.Type.Should().Be(PackageVersionType.SemVer);
    }

    [Fact]
    public void Value_ReassignedToStar_ClearsParsedProperties()
    {
        var sut = new PackageVersion("1.0.0");
        sut.Type.Should().Be(PackageVersionType.SemVer);

        sut.Value = "*";

        sut.Type.Should().Be(PackageVersionType.Star);
        sut.Major.Should().BeNull();
        sut.IsCatchAll.Should().BeTrue();
    }

    #endregion
}
