using NSL.Generators.BinaryTypeIOGenerator.Attributes;

namespace NSL.Generators.RpcGenerator.Tests.Shared
{
    // Simple model serialized via BinaryGenerator (full mode)
    [NSLBIOType]
    public partial class PlayerInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }
    }

    // Model without NSLBIO — serialized field-by-field via primitive read/write
    public class ChatMessage
    {
        public string Author { get; set; }
        public string Text { get; set; }
        public string Channel { get; set; }
    }
}
