using System;

namespace NSL.ASPNET.Localization.Shared
{
    public abstract class BaseStaticLocalizationItemModel : StaticLocalizationIdentityModel
    {
        /*[SelectGenerateInclude("Get")] */
        public DateTime LatestModified { get; set; }

        /*[SelectGenerateInclude("Get")] */
        public string Value { get; set; }

        public bool ClientValue { get; set; }
    }
}
