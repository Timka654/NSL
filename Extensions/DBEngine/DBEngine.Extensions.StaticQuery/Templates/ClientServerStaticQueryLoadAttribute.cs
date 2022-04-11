namespace NSL.Extensions.DBEngine.StaticQuery.Templates
{
    public class ClientServerStaticQueryLoadAttribute : StaticQueryLoadAttribute
    {
        public ClientServerStaticQueryLoadAttribute(int order, string name) : base(order, name)
        {
        }
    }
}
