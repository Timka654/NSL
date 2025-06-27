namespace NSL.Database.EntityFramework.Filter.Tests
{
    public class TestEntityModel
    {
        public Guid Id { get; set; }

        public string Content { get; set; }

        public DateTime? NullCheckDate { get; set; }

        public virtual List<RelTestEntityModel>? RelTests { get; set; }
    }


    public class RelTestEntityModel
    {
        public Guid Id { get; set; }

        public int Type { get; set; }

        public string Content { get; set; }

        public Guid TestId { get; set; }

        public virtual TestEntityModel? Test { get; set; }
    }
}
