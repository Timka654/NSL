using System.Threading.Tasks;

namespace NSL.ASPNET.Blazor.Components
{
    public interface ISidebarComponent
    {
        Task RefreshDataAsync(bool reloadData = true);
        void ToggleSidebar();
    }
}
