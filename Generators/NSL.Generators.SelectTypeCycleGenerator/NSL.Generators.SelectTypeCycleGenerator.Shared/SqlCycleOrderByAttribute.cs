using System;

namespace NSL.Entity.SelectCycleGenerator.Shared
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class SqlCycleOrderByAttribute : Attribute
    {
        public SqlCycleOrderByAttribute(params string[] models)
        {
            Models = models;
        }

        public string[] Models { get; }

        /// <summary>
        /// True = DESC, False = ASC (по умолчанию)
        /// </summary>
        public bool Descending { get; set; } = true;

        /// <summary>
        /// Порядок применения сортировки (чем меньше, тем раньше)
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// 0 = Both, 1 = Anchor, 2 = Recursion
        /// </summary>
        public SqlCycleFilterTarget Target { get; set; } = SqlCycleFilterTarget.Both;
    }
}
