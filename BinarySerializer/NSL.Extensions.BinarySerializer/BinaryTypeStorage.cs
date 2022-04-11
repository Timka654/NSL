using System.Reflection;

namespace NSL.Extensions.BinarySerializer
{
    public class BinaryTypeStorage : BinaryType
    {
        public BinaryReadAction ReadAction;
        public BinaryWriteAction WriteAction;

        public override BinaryReadAction GetReadAction(string scheme = "default", PropertyInfo property = null)
        {
            return ReadAction;
        }

        public override BinaryWriteAction GetWriteAction(string scheme = "default", PropertyInfo property = null)
        {
            return WriteAction;
        }

        public override void RegisterScheme(BinaryScheme scheme)
        {

        }
    }
}
