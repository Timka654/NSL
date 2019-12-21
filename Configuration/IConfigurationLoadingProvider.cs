using System;
using System.Collections.Generic;
using System.Text;

namespace ConfigurationEngine
{
    public interface IConfigurationLoadingProvider
    {
        bool LoadData(IConfigurationManager manager);
    }
}
