using NSL.Utils;
using NSL.Utils.CommandLine;

namespace NSL.ConfigurationEngine.Providers
{
    public class CommandLineArgsConfigurationProvider : BaseConfigurationProvider
    {
        public override bool LoadData()
        {
            foreach (var item in (new CommandLineArgs()).GetArgs())
            {
                Update(new Info.ConfigurationInfo(item.Key.Replace("__", "."), item.Value, this, ""), true);
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
