using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace ServerOptions.Extensions.StaticQuery
{
    public interface IStaticQuery
    {
        void Run(DbConnection connection);
    }
}
