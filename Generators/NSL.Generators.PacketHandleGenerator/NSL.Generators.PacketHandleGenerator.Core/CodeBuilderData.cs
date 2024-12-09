using NSL.Generators.Utils;

namespace NSL.Generators.PacketHandleGenerator
{
    internal class CodeBuilderData
    {
        public CodeBuilder PacketHandlesBuilder { get; set; } = new CodeBuilder();

        public CodeBuilder HandlesBuilder { get; set; } = new CodeBuilder();

        public CodeBuilder ConfigureBuilder { get; set; } = new CodeBuilder();
    }
}
