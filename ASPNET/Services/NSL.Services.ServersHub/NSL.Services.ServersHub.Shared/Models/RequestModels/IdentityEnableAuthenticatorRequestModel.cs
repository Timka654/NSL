using System.ComponentModel.DataAnnotations;

namespace NSL.Services.ServersHub.Shared.Models.RequestModels
{
    public partial class IdentityEnableAuthenticatorRequestModel
    {
        [Required]
        [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Verification Code")]
        public string Code { get; set; } = "";
    }
}