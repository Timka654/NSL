using System.Threading.Tasks;

namespace NSL.ASPNET.Blazor.Context
{
    public interface IInitializingComponentContext : IComponentContext
    {
        Task InitializeAsync();
    }
}
