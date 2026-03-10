using Microsoft.AspNetCore.Components;

namespace NSL.ASPNET.Blazor.Components
{
    public interface IRightSidebarComponent
    {
        void ShowContent(RenderFragment renderFragment, string title, string titleLK);

        void Update();
    }
}
