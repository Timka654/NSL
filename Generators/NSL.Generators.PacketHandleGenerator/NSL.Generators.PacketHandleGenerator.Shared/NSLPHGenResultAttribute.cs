﻿using System;

namespace NSL.Generators.PacketHandleGenerator.Shared
{
    [AttributeUsage(AttributeTargets.Field)]
    public class NSLPHGenResultAttribute : Attribute
    {
        public NSLPHGenResultAttribute(Type type)
        {

        }
    }
}
