namespace ServerOptions.Extensions.StaticQuery.Templates
{
    public class ClientServerStaticQueryLoadAttribute : StaticQueryLoadAttribute
    {
        public ClientServerStaticQueryLoadAttribute(int order, string name) : base(order, name)
        {
        }
    }
}
