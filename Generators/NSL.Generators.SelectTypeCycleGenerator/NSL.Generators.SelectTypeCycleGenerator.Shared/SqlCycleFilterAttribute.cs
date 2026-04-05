using System;

namespace NSL.Generators.SelectTypeCycleGenerator.Shared
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class SqlCycleFilterAttribute : Attribute
    {
        public SqlCycleFilterAttribute(params string[] models)
        {
            Models = models;
        }

        public string[] Models { get; }

        // По умолчанию оставляем Both для обратной совместимости
        public SqlCycleFilterTarget Target { get; set; } = SqlCycleFilterTarget.Anchor;
    }
}
