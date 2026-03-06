//-:cnd:noEmit
#if NSL_SERVER

using Microsoft.AspNetCore.Identity;
using NSL.Generators.SelectTypeGenerator.Attributes;

namespace NSL.BlazorTemplate.Shared.Models
{
    [SelectGenerate("Details")]
    [SelectGenerateModelJoin("Details", "BaseGet")]
    public partial class UserModel : IdentityUser<Guid>
    {
        [SelectGenerateInclude("BaseGet")]
        public override string? Email { get => base.Email; set => base.Email = value; }

        [SelectGenerateInclude("Details")]
        public override string? PhoneNumber { get => base.PhoneNumber; set => base.PhoneNumber = value; }

        [SelectGenerateInclude("Details")]
        public override string? UserName { get => base.UserName; set => base.UserName = value; }

        [SelectGenerateInclude("Details")]
        public override bool EmailConfirmed { get => base.EmailConfirmed; set => base.EmailConfirmed = value; }
    }
}

#endif
//+:cnd:noEmit