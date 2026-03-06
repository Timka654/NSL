namespace NSL.Database.EntityFramework.Logger.Shared
{
    public class ChangePropertyModel
    {
        public string PropertyName { get; set; }

        public object? OldValue { get; set; }

        public object? NewValue { get; set; }

        public override string ToString()
            => $"{PropertyName} : {OldValue} > {NewValue}";
    }
}
