namespace DIEngine
{
    public interface IDependencyObject
    {
        void OnLoaded(DependencyInjection di);
    }
}
