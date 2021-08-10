namespace ServerOptions.Extensions.StaticQuery.Templates
{
    public class AuthServerStaticQueryLoadAttribute : StaticQueryLoadAttribute
    {
        public AuthServerStaticQueryLoadAttribute(int order, string name) : base(order, name)
        {
        }
    }
}
