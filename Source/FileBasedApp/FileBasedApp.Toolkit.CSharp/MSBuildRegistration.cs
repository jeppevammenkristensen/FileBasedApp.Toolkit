using Microsoft.Build.Locator;

namespace FileBasedApp.Toolkit.CSharp;

/// <summary>Coordinates MSBuild registration across project and solution loaders.</summary>
internal static class MSBuildRegistration
{
    // A lock on BaseAnalysis<TSelf> would be separate for each closed generic type.
    private static readonly object RegistrationLock = new();

    /// <summary>Registers the default MSBuild instance once before workspaces are created.</summary>
    internal static void EnsureRegistered()
    {
        lock (RegistrationLock)
        {
            if (MSBuildLocator.IsRegistered)
            {
                return;
            }

            if (!MSBuildLocator.QueryVisualStudioInstances().Any())
            {
                throw new InvalidOperationException(
                    "No MSBuild instances were found. Ensure that a .NET SDK or Visual Studio is installed. " +
                    "If running in a file-based app, make sure the .NET SDK is on the PATH.");
            }

            MSBuildLocator.RegisterDefaults();
        }
    }
}
