using System;
using System.Collections.Generic;
using System.Text;

namespace DIEngine
{
    public interface IDependencyObject
    {
        void OnLoaded(DependencyInjection di);
    }
}
