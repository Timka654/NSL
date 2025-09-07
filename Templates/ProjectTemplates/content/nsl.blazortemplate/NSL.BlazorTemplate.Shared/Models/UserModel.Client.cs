//-:cnd:noEmit
#if NSL_CLIENT

namespace NSL.BlazorTemplate.Shared.Models
{
    public partial class UserModel
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public bool EmailConfirmed { get; set; }
    }
}

#endif
//+:cnd:noEmit