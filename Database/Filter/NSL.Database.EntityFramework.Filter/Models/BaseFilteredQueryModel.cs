namespace NSL.Database.EntityFramework.Filter.Models
{
    public class BaseFilteredQueryModel
    {
        public int Offset { get; set; } = 0;

        public int Count { get; set; } = int.MaxValue;

        public List<FilterBlockViewModel>? FilterQuery { get; set; }

        public List<FilterBlockOrderViewModel>? OrderQuery { get; set; }
    }
}
