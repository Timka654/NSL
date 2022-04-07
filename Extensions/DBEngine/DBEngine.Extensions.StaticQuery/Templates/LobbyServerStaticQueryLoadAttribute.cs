namespace ServerOptions.Extensions.StaticQuery.Templates
{
    public class LobbyServerStaticQueryLoadAttribute : StaticQueryLoadAttribute
    {
        public LobbyServerStaticQueryLoadAttribute(int order, string name) : base(order, name)
        {
        }
    }
}
