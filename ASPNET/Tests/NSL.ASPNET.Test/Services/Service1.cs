using Microsoft.Extensions.Options;
using NSL.ASPNET.Attributes;

namespace NSL.ASPNET.Test.Services
{
    [RegisterService(ServiceLifetime.Singleton, "basicModel")]
    public class basicService1
    {
    }

    [RegisterService(ServiceLifetime.Singleton, "basicModel")]
    public class basicService2(basicService1 service1)
    {
        public basicService1 Service1 { get; } = service1;
    }

    [RegisterService("key1", ServiceLifetime.Singleton, "keyModel", "errorKeyModel")]
    public class keyService1(basicService1 service1, basicService2 service2)
    {
        public basicService1 Service1 { get; } = service1;
        public basicService2 Service2 { get; } = service2;
    }

    [RegisterService(ServiceLifetime.Singleton, "keyModel")]
    [RegisterService(ServiceLifetime.Singleton, "errorKeyModel")]
    public class keyService2([FromKeyedServices("key1")] keyService1 service1)
    {
        public keyService1 Service1 { get; } = service1;
    }

    [RegisterService(ServiceLifetime.Singleton, "errorKeyModel")]
    public class keyService3(keyService1 service3)
    {
        public keyService1 Service3 { get; } = service3;
    }

    [RegisterService(ServiceLifetime.Singleton, "lifeTimeConflictModel")]
    public class lifeTimeConflictService1(lifeTimeConflictService2 service2)
    {
        public lifeTimeConflictService2 Service2 { get; } = service2;
    }

    [RegisterService(ServiceLifetime.Scoped, "lifeTimeConflictModel")]
    public class lifeTimeConflictService2
    {
    }

    [RegisterService(ServiceLifetime.Singleton, "optionsRequiredModel")]
    public class optionsRequiredService1(IOptions<testOptions> options, IOptionsMonitor<testOptions> optionsMonitor)
    {
        public IOptions<testOptions> Options { get; } = options;
        public IOptionsMonitor<testOptions> OptionsMonitor { get; } = optionsMonitor;
    }

    [RegisterService(ServiceLifetime.Singleton, "serviceProviderRequiredModel")]
    public class serviceProviderRequiredService1(IServiceProvider serviceProvider)
    {
    }

    public class testOptions
    {
        public string Test { get; set; } = string.Empty;
    }
}
