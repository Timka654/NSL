using Microsoft.Extensions.DependencyInjection;
using System;

namespace NSL.ASPNET.Attributes
{
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false)]
    public class RegisterServiceConstructorAttribute() : Attribute { }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RegisterServiceAttribute(ServiceLifetime type, params string[] models) : Attribute
    {
        public RegisterServiceAttribute(string key, ServiceLifetime type, params string[] models) : this(type, models)
        {
            Key = key;
        }

        public ServiceLifetime Type { get; } = type;

        public string[] Models { get; } = models;

        public string? Key { get; }
    }
}
