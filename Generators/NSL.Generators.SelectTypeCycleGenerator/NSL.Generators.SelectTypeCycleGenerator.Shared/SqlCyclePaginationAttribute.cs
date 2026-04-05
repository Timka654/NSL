using System;

namespace NSL.Generators.SelectTypeCycleGenerator.Shared
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SqlCyclePaginationAttribute : Attribute
    {
        public SqlCyclePaginationAttribute(bool skip, bool take, params string[] models)
        {
            Skip = skip;
            Take = take;
            Models = models;
        }

        public bool Skip { get; }
        public bool Take { get; }

        // По умолчанию применяем пагинацию только к якорю (корневым элементам)
        public SqlCycleFilterTarget Target { get; set; } = SqlCycleFilterTarget.Anchor;

        public string[] Models { get; }
    }
}
