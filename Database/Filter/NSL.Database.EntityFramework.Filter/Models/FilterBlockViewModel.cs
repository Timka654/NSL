using NSL.Database.EntityFramework.Filter.Enums;

namespace NSL.Database.EntityFramework.Filter.Models
{
    public class FilterBlockViewModel
    {
        public List<FilterPropertyViewModel> Propertyes { get; set; } = new List<FilterPropertyViewModel>();

        public FilterBlockViewModel AddFilter(string propertyPath, CompareType compareType, object value)
        {
            Propertyes.Add(new FilterPropertyViewModel()
            {
                PropertyPath = propertyPath,
                CompareType = compareType,
                Value = value.ToString()
            });

            return this;
        }
    }
}
