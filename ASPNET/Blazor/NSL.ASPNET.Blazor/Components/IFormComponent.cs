using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.ASPNET.Blazor.Components
{
    public interface IFormComponent
    {
        public bool EditState { get; }
        public event Action<bool> OnEditStateChanged;
    }
}
