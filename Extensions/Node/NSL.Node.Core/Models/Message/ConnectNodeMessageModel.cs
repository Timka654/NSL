using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Node.Core.Models.Message
{
    [NSLBIOType]
    public partial class ConnectNodeMessageModel
    {
        public string NodeId { get; set; }
        public string Token { get; set; }
        public string EndPoint { get; set; }
    }
}
