﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SCL.SocketClient.Utils
{
    /// <summary>
    /// Ошибка хеша пакета
    /// </summary>
    public class InvalidPacketHashException : Exception
    {
        public InvalidPacketHashException() : base("Ошибка проверки хеша")
        {

        }
    }
}
