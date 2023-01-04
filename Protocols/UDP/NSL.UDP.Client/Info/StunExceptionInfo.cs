using STUN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NSL.UDP.Client.Info
{
    public class StunExceptionInfo : Exception
    {
        public StunExceptionInfo(StunServerInfo serverInfo, STUNQueryResult queryResult, ErrorTypeEnum errorType, IPEndPoint selectedEndPoint, string message = default) : this(queryResult, errorType, message)
        {
            ServerInfo = serverInfo;
            SelectedEndPoint = selectedEndPoint;
        }

        private StunExceptionInfo(STUNQueryResult queryResult, ErrorTypeEnum errorType, string message = default) : base(message)
        {
            QueryResult = queryResult;
            ErrorType = errorType;
        }

        public STUNQueryResult QueryResult { get; }

        public StunServerInfo ServerInfo { get; }

        public ErrorTypeEnum ErrorType { get; }

        public IPEndPoint SelectedEndPoint { get; }

        public enum ErrorTypeEnum
        {
            QueryResultError,
            DNSIPAddressParseError
        }
    }
}
