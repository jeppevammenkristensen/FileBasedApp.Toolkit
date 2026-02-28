using System.ComponentModel;
using TruePath;

namespace FileBasedApp.Toolkit;

[TypeConverter]
public record EvaluatedPath(string? OriginalPath, AbsolutePath? Path, bool Exists)
{   
    public AbsolutePath GetPath(bool shouldExist, bool originalPathCanBeNull)
    {
        if (!originalPathCanBeNull && string.IsNullOrWhiteSpace(OriginalPath))
        {
            throw new InvalidOperationException("OriginalPath cannot be null");
        }

        if (shouldExist && !Exists)
        {
            throw new InvalidOperationException($"Path {OriginalPath} translated to {Path} does not exist");
        }
        
        if (Path is null)
        {
            throw new InvalidOperationException($"Path {OriginalPath} could not be translated to an AbsolutePath");
        }

        

        return Path.Value;

    }
}