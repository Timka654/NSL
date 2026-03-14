using System.Collections.Generic;
using System.Text;

namespace NSL.ASPNET.Blazor.Components
{
    public delegate void EditStateChangedDelegate(bool editState);

    public interface IFormComponent
    {
        public bool EditState { get; }

        public event EditStateChangedDelegate OnEditStateChanged;
    }
}
