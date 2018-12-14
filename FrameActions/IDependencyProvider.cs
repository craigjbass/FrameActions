using Optional;

namespace FrameActions
{
    public interface IDependencyProvider
    {
        Option<T> Get<T>();
    }
}