namespace NSL.Extensions.DIEngine
{
    public interface IDependencyObject
    {
        void OnLoaded(DependencyInjection di);
    }
}
