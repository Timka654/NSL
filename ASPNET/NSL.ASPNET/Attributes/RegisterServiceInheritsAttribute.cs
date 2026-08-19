using Microsoft.Extensions.DependencyInjection;
using System;

namespace NSL.ASPNET.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RegisterServiceInheritsAttribute(ServiceLifetime type, Type inherit) : Attribute
    {
        public RegisterServiceInheritsAttribute(object key, ServiceLifetime type, Type inherit) : this(type, inherit)
        {
            Key = key;
        }

        public ServiceLifetime Type { get; } = type;

        public Type Inherit { get; } = inherit;

        public object? Key { get; }
    }
}
