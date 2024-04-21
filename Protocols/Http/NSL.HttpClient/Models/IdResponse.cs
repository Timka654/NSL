using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevExtensions.Blazor.Http.Models
{
    public class IdResponse<TId> : BaseResponse
    {
        public TId Id { get; set; }
    }
}
