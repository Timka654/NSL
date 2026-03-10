using Microsoft.AspNetCore.Components.Web;
using NSL.ASPNET.Blazor.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.ASPNET.Blazor.Services
{
    public delegate void LayoutClickEventHandler(bool left);

    public class NSLLayoutService
    {
        public event LayoutClickEventHandler OnClick = (left) => { };

        public ISidebarComponent RootSidebar { get; set; }

        public IRightSidebarComponent RootRightSidebar { get; set; }

        public void LayoutClickBroadcast(bool left)
        {
            OnClick(left);
        }
    }
}
