using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Database.EntityFramework.Filter.Models
{
    public class FilterResultModel<TData>
    {
        public TData[] Data { get; set; }

        public long Count { get; set; }
    }
}
