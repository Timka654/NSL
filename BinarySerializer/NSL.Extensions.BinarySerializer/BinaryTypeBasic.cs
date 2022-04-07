using System.Reflection;

namespace NSL.Extensions.BinarySerializer
{
    public class BinaryTypeBasic : BinaryType
    {
        public override void RegisterScheme(BinaryScheme scheme)
        {

        }

        public override BinaryReadAction GetReadAction(string scheme = "default", PropertyInfo property = null)
        {
            return null;
        }

        public override BinaryWriteAction GetWriteAction(string scheme = "default", PropertyInfo property = null)
        {
            return null;
        }
    }
}
