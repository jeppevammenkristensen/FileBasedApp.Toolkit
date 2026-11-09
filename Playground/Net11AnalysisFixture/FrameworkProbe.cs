namespace Net11AnalysisFixture;

/// <summary>Provides a symbol that exists only when the fixture targets .NET 11.</summary>
#if NET11_0
public sealed class FrameworkProbe
{
    /// <summary>Gets the framework version used to compile this fixture.</summary>
    public const int FrameworkMajor = 11;
}
#else
#error This analysis fixture must target .NET 11.
#endif
