using System;
using System.Collections;

namespace NSL.ConfigurationEngine.Providers
{
    public class EnvironmentVariableConfigurationProvider : BaseConfigurationProvider
    {
        public override bool LoadData()
        { 
            Hashtable table = Environment.GetEnvironmentVariables() as Hashtable;

            foreach (string item in table.Keys)
            {
                Update(new Info.ConfigurationInfo(item.Replace("__", "."), (string)table[item], this, ""), true);
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
