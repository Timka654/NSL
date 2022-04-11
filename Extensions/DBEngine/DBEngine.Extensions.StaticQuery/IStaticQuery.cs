
namespace NSL.Extensions.DBEngine.StaticQuery
{
    public interface IStaticQuery
    {
        void Run(DbConnectionPool connectionPool);
    }
}
