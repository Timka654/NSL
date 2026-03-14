namespace NSL.ASPNET.Localization.Shared
{
    public class BaseCreateLocalizationItemRequestModel
    {
        public string Key { get; set; }
        public string? Language { get; set; }
        public string Value { get; set; }
    }
}
