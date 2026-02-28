using System.Text;

namespace GetFileSystemSourceAndParse;

public class BuilderWrapper : IDisposable
{
    private readonly Action _endAction;

    public BuilderWrapper(StringBuilder builder, Action<StringBuilder> start, Action<StringBuilder> end, bool condition = true)
    {
        if (condition)
            start(builder);
        _endAction = () =>
        {
            if (condition)
                end(builder);
        };
    }
    
    
    
    public void Dispose()
    {
        _endAction();
    }
}