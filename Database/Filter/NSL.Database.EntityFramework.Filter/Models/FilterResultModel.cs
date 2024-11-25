namespace NSL.Database.EntityFramework.Filter.Models
{
    public class FilterResultModel<TData>
    {
        public TData[] Data { get; set; }

        public long Count { get; set; }
    }
}
