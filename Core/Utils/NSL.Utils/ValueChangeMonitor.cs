using System;

namespace NSL.Utils
{
    public struct ValueChangeMonitor
    {
        private readonly Func<object> a;

        object currentValue;

        public ValueChangeMonitor(Func<object> a)
        {
            this.a = a;
            currentValue = a();
        }

        public object OldValue => currentValue;

        public bool IsChanged()
            => !Equals(currentValue, a());

        public bool IsEquals()
            => !Equals(currentValue, a());

        public bool IsSetNull()
            => currentValue != null && a() == null;

        public bool IsSetValue()
            => currentValue == null && a() != null;
    }
}
