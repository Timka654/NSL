using Microsoft.Extensions.DependencyInjection;
using System;

namespace NSL.ASPNET.Attributes
{
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false)]
    public class RegisterServiceConstructorAttribute() : Attribute { }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RegisterServiceAttribute(ServiceLifetime type, params string[] models) : Attribute
    {
        public RegisterServiceAttribute(object key, ServiceLifetime type, params string[] models) : this(type, models)
        {
            Key = key;
        }

        public ServiceLifetime Type { get; } = type;

        public string[] Models { get; } = models;

        public object? Key { get; }

        public bool HostedService { get; set; } = false;
    }

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
