namespace FrameActions
{
    public interface IFrameAction<in TRequestType>
    {
        void NewFrame(TRequestType payload);
    }
}