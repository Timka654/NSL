﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SocketCore.Utils.Exceptions
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