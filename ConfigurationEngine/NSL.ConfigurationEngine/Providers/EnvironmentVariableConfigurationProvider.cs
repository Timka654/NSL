using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.ConfigurationEngine.Providers
{
    public class EnvironmentVariableConfigurationProvider : BaseConfigurationProvider
    {
        public override bool LoadData()
        { 
            Hashtable table = Environment.GetEnvironmentVariables() as Hashtable;

            foreach (string item in table.Keys)
            {
                Update(new Info.ConfigurationInfo(item, (string)table[item], this, ""), true);
            }

            return true;
        }

        public override bool LoadData(byte[] data)
        {
            return true;
        }

        public override bool SaveData()
        {
            return true;
        }
    }
}
