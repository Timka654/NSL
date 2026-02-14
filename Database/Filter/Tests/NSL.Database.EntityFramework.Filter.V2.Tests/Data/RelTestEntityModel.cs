namespace NSL.Database.EntityFramework.Filter.V2.Tests.Data
{
    public class RelTestEntityModel
    {
        public Guid Id { get; set; }

        public int Type { get; set; }

        public string Content { get; set; }

        public Guid TestId { get; set; }

        public virtual TestEntityModel? Test { get; set; }
    }
}
