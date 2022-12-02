using STUN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.UDP.Client.Info
{
    public class StunExceptionInfo : Exception
    {
        public StunExceptionInfo(STUNQueryResult queryResult)
        {
            QueryResult = queryResult;
        }

        public STUNQueryResult QueryResult { get; }
    }
}
