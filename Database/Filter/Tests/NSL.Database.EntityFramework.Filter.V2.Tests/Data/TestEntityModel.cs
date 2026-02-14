namespace NSL.Database.EntityFramework.Filter.V2.Tests.Data
{
    public class TestEntityModel
    {
        public Guid Id { get; set; }

        public string Content { get; set; }

        public string? NullContent { get; set; }

        public int Number { get; set; }
        public double FloatNumber { get; set; }

        public int? NullNumber { get; set; }
        public double? NullFloatNumber { get; set; }

        public bool BooleanValue { get; set; }
        public bool? NullableBooleanValue { get; set; }

        public DateTime? NullCheckDate { get; set; }

        public DateTime CheckDate { get; set; } = DateTime.MinValue;

        public TestEnum? NullEnum { get; set; }

        public TestEnum Enum { get; set; }

        public virtual List<RelTestEntityModel>? RelTests { get; set; }
    }
}
