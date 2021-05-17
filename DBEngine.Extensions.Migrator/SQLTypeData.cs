using System;

namespace DBEngine.DBMigrator
{
    public class SQLTypeData
    {
        public string Type { get; private set; }

        public string NullSegment { get; private set; }

        public Func<object, string> Getter { get; private set; }

        public Func<object> DefaultValue { get; private set; }

        public SQLTypeData(string type, string nullSegment, Func<object, string> getter, Func<object> defaultValue)
        {
            Type = type;
            NullSegment = nullSegment;
            Getter = getter;
            DefaultValue = defaultValue;
        }
    }
}
