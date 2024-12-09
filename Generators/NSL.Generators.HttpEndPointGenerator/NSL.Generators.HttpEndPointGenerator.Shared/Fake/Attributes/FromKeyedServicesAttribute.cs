using System;

namespace NSL.Generators.HttpEndPointGenerator.Shared.Fake.Attributes
{
    /// <summary>
    /// Fake attribute class for generator success build
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromKeyedServicesAttribute : Attribute
    {
        //
        // Сводка:
        //     Creates a new Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute
        //     instance.
        //
        // Параметры:
        //   key:
        //     The key of the keyed service to bind to.
        public FromKeyedServicesAttribute(object key) { }

        //
        // Сводка:
        //     The key of the keyed service to bind to.
        public object Key { get; }
    }
}
