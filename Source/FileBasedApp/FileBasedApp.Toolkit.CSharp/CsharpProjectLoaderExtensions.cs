using TruePath;

namespace FileBasedApp.Toolkit.CSharp;

/// <summary>
/// Provides extension methods for <see cref="CsharpProjectAnalysis"/> to enhance its functionality.
/// </summary>
/// <remarks>
/// This static class contains utility extension methods that extend the capabilities of the CsharpProjectLoader class.
/// </remarks>
public static class CsharpProjectLoaderExtensions
{
    /// <param name="cls"></param>
    /// <typeparam name="T"></typeparam>
    extension<T>(T cls) where T : CsharpProjectAnalysis
    {
        /// <summary>
        /// Adds or updates a MSBuild property that will be used during project loading.
        /// If the property already exists, its value will be updated; otherwise, a new property will be added.
        /// </summary>
        /// <param name="name">The name of the MSBuild property to add or update.</param>
        /// <param name="value">The value to assign to the property.</param>
        /// <returns>The current instance of <see cref="CsharpProjectAnalysis"/> for method chaining.</returns>
        public T WithProperty(string name, string value)
        {
            if (cls.Properties.ContainsKey(name))
            {
                cls.Properties = cls.Properties.SetItem(name, value);
            }
            else
            {
                cls.Properties = cls.Properties.Add(name, value);
            }

            return cls;
        }

        /// <summary>
        /// Loads the C# project from the specified path asynchronously and returns the current instance for method chaining.
        /// </summary>
        /// <param name="path">The absolute path to the C# project file to load.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests during the loading operation.</param>
        /// <returns>A task that represents the asynchronous load operation. The task result contains the current instance of <see cref="CsharpProjectAnalysis"/> for method chaining.</returns>
        /// <remarks>If the project is succesfully loaded. Workspace, Project and Compillation will be available/></remarks>
        public async Task<T> Load(AbsolutePath path, CancellationToken cancellationToken = default)
        {
            await cls.InternalLoad(path,cancellationToken);
            return cls;
        }

        /// <summary>
        /// Uses the provided visitor to traverse the compilation tree and collect results.
        /// </summary>
        /// <param name="visitor"></param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public IEnumerable<TResult> GetResult<TResult>(BaseSymbolVisitor<TResult> visitor)
        {
            return visitor.VisitAsEnumerable(cls.Compilation.GlobalNamespace);
        }
    }
}