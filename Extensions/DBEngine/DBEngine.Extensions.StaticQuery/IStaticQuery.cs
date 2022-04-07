using DBEngine;

namespace ServerOptions.Extensions.StaticQuery
{
    public interface IStaticQuery
    {
        void Run(DbConnectionPool connectionPool);
    }
}
