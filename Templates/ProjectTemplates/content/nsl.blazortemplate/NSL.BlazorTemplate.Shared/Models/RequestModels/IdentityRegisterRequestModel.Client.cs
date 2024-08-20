#if CLIENT
using System.ComponentModel.DataAnnotations;

namespace NSL.BlazorTemplate.Shared.Models.RequestModels
{
    public partial class IdentityRegisterRequestModel
    {
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = "";
    }
}

#endif