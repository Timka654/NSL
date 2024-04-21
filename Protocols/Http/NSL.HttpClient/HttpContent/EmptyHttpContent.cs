using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevExtensions.Blazor.Http.HttpContent
{
    public class EmptyHttpContent : StringContent
    {
        public static EmptyHttpContent Instance { get; } = new EmptyHttpContent();

        public EmptyHttpContent() : base("")
        {

        }
    }
}
