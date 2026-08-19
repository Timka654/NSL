using System;

namespace NSL.ASPNET.Attributes
{
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false)]
    public class RegisterServiceConstructorAttribute() : Attribute { }
}
