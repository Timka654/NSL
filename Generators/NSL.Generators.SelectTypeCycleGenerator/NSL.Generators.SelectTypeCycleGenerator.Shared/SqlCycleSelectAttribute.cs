using System;

namespace NSL.Entity.SelectCycleGenerator.Shared
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SqlCycleSelectAttribute : Attribute
    {
        public string[] Models { get; }
        public SqlCycleSelectAttribute(params string[] models) => Models = models;
    }
}
