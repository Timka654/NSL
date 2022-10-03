using System;

namespace NSL.Extensions.RPC.Generator.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class RPCCustomMemberIgnoreAttribute : Attribute
    {
        /// <summary>
        /// ignore type inner members on IO
        /// </summary>
        /// <param name="members"></param>
        public RPCCustomMemberIgnoreAttribute(params string[] members)
        {
            Members = members;
        }

        /// <summary>
        /// replace parameter with default
        /// </summary>
        public RPCCustomMemberIgnoreAttribute()
        {
            Members = new string[] { "*" };
        }

        public string[] Members { get; }
    }
}
