﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.UDP.Client.Info
{
    public class StunServerInfo
    {
        public StunServerInfo(string endPoint)
        { 
            var splited = endPoint.Split(':');

            string address;

            int port = 3478;

            if (splited.Length > 1)
            {
                address = string.Join(":", splited.Take(splited.Length - 1));
                port = int.Parse(splited[splited.Length - 1]);
            }
            else
                address = endPoint;

            Address = address;
            Port = port;
        }

        public StunServerInfo(string address, int port)
        {
            Address = address;
            Port = port;
        }

        public string Address { get; }
        public int Port { get; }
    }
}
